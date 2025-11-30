using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Orders;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public OrdersController(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderListDto>>> GetOrders()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.Shop)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var orderDtos = orders.Select(o => new OrderListDto
        {
            OrderId = o.OrderId,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            TotalItems = o.ShopOrders.SelectMany(so => so.OrderItems).Sum(i => i.Quantity),
            Status = GetOverallStatus(o.ShopOrders),
            Shops = o.ShopOrders.Select(so => new OrderShopSummaryDto
            {
                ShopId = so.ShopId,
                ShopName = so.Shop.ShopName,
                Status = so.ShopOrderStatus,
                ItemCount = so.OrderItems.Sum(i => i.Quantity)
            }).ToList()
        }).ToList();

        return Ok(orderDtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _context.Orders
            .Where(o => o.OrderId == id && o.UserId == userId)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Discount)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.Shop)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(MapToDetailDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Verify shipping address belongs to user
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.AddressId == request.ShippingAddressId && a.UserId == userId);

        if (address == null)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = "Invalid shipping address"
            });
        }

        // Get cart items
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
                .ThenInclude(p => p.Shop)
            .ToListAsync();

        if (!cartItems.Any())
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = "Cart is empty"
            });
        }

        // Validate stock
        foreach (var item in cartItems)
        {
            if (!item.Product.IsActive || !item.Product.Shop.IsApproved)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = $"Product '{item.Product.ProductName}' is no longer available"
                });
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = $"Insufficient stock for '{item.Product.ProductName}'. Available: {item.Product.StockQuantity}"
                });
            }
        }

        // Check discount if provided
        Discount? discount = null;
        if (!string.IsNullOrEmpty(request.DiscountCode))
        {
            discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == request.DiscountCode &&
                    d.IsActive &&
                    (d.ExpiresAt == null || d.ExpiresAt >= DateTime.UtcNow));

            if (discount == null)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Invalid or expired discount code"
                });
            }

            // Check usage limit
            if (discount.UsageLimit.HasValue && discount.UsageCount >= discount.UsageLimit)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Discount code usage limit reached"
                });
            }
        }

        // Calculate totals
        var subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity);
        var total = subtotal;

        if (discount != null)
        {
            if (subtotal < discount.MinimumOrderAmount)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = $"Minimum order amount of {discount.MinimumOrderAmount:C} required for this discount"
                });
            }

            total = discount.DiscountType.ToLower() == "percentage"
                ? subtotal * (1 - discount.Value / 100)
                : subtotal - discount.Value;

            if (total < 0) total = 0;
        }

        // Create order
        var order = new Order
        {
            UserId = userId.Value,
            ShippingAddressId = request.ShippingAddressId,
            OrderDate = DateTime.UtcNow,
            SubtotalAmount = subtotal,
            DiscountId = discount?.DiscountId,
            TotalAmount = total
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Group cart items by shop
        var itemsByShop = cartItems.GroupBy(c => c.Product.ShopId);

        foreach (var shopGroup in itemsByShop)
        {
            var shopSubtotal = shopGroup.Sum(c => c.Product.Price * c.Quantity);

            var shopOrder = new ShopOrder
            {
                OrderId = order.OrderId,
                ShopId = shopGroup.Key,
                ShopOrderStatus = "pending",
                ShopSubtotal = shopSubtotal,
                ShopTotal = shopSubtotal // Individual shop discounts could be applied here
            };

            await _unitOfWork.ShopOrders.AddAsync(shopOrder);
            await _unitOfWork.SaveChangesAsync();

            // Add order items
            foreach (var cartItem in shopGroup)
            {
                var orderItem = new OrderItem
                {
                    ShopOrderId = shopOrder.ShopOrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Product.Price
                };

                await _unitOfWork.OrderItems.AddAsync(orderItem);

                // Decrease stock
                cartItem.Product.StockQuantity -= cartItem.Quantity;
                _context.Products.Update(cartItem.Product);
            }
        }

        // Clear cart
        _unitOfWork.CartItems.RemoveRange(cartItems);
        await _unitOfWork.SaveChangesAsync();

        // Reload order with all details
        var createdOrder = await _context.Orders
            .Where(o => o.OrderId == order.OrderId)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Discount)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.Shop)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .FirstAsync();

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order created successfully",
            OrderId = order.OrderId,
            Order = MapToDetailDto(createdOrder)
        });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderResponse>> CancelOrder(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _context.Orders
            .Where(o => o.OrderId == id && o.UserId == userId)
            .Include(o => o.ShopOrders)
                .ThenInclude(so => so.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new OrderResponse
            {
                Success = false,
                Message = "Order not found"
            });
        }

        // Check if any shop order can be cancelled
        var cancellableStatuses = new[] { "pending", "confirmed" };
        var hasCancellableItems = order.ShopOrders.Any(so => cancellableStatuses.Contains(so.ShopOrderStatus.ToLower()));

        if (!hasCancellableItems)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = "Order cannot be cancelled. It may already be shipped or delivered."
            });
        }

        // Cancel each shop order and restore stock
        foreach (var shopOrder in order.ShopOrders.Where(so => cancellableStatuses.Contains(so.ShopOrderStatus.ToLower())))
        {
            shopOrder.ShopOrderStatus = "cancelled";

            // Restore stock
            foreach (var item in shopOrder.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
                _context.Products.Update(item.Product);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order cancelled successfully"
        });
    }

    private static string GetOverallStatus(ICollection<ShopOrder> shopOrders)
    {
        if (!shopOrders.Any()) return "unknown";

        var statuses = shopOrders.Select(so => so.ShopOrderStatus.ToLower()).Distinct().ToList();

        if (statuses.All(s => s == "delivered")) return "delivered";
        if (statuses.All(s => s == "cancelled")) return "cancelled";
        if (statuses.Any(s => s == "shipped")) return "shipped";
        if (statuses.Any(s => s == "processing")) return "processing";
        if (statuses.Any(s => s == "confirmed")) return "confirmed";
        return "pending";
    }

    private static OrderDetailDto MapToDetailDto(Order order)
    {
        return new OrderDetailDto
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            SubtotalAmount = order.SubtotalAmount,
            TotalAmount = order.TotalAmount,
            DiscountCode = order.Discount?.Code,
            ShippingAddress = new AddressDto
            {
                AddressId = order.ShippingAddress.AddressId,
                AddressLabel = order.ShippingAddress.AddressLabel,
                StreetAddress = order.ShippingAddress.StreetAddress,
                City = order.ShippingAddress.City,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country
            },
            ShopOrders = order.ShopOrders.Select(so => new ShopOrderDto
            {
                ShopOrderId = so.ShopOrderId,
                ShopId = so.ShopId,
                ShopName = so.Shop.ShopName,
                LogoImageUrl = so.Shop.LogoImageUrl,
                Status = so.ShopOrderStatus,
                TrackingNumber = so.TrackingNumber,
                Subtotal = so.ShopSubtotal,
                Total = so.ShopTotal,
                Items = so.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    Slug = oi.Product.Slug,
                    MainImageUrl = oi.Product.MainImageUrl,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase
                }).ToList()
            }).ToList()
        };
    }
}
