using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Seller;

[ApiController]
[Route("api/seller")]
[Authorize(Roles = "seller")]
public class SellerController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public SellerController(IUnitOfWork unitOfWork, AppDbContext context)
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

    private async Task<Shop?> GetUserShop(int userId)
    {
        return await _context.Shops.FirstOrDefaultAsync(s => s.SellerId == userId);
    }

    #region Shop Management

    [HttpGet("shop")]
    public async Task<ActionResult<ShopDto>> GetShop()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "You don't have a shop. Create one first." });
        }

        return Ok(MapToShopDto(shop));
    }

    [HttpPost("shop")]
    public async Task<ActionResult<ShopDto>> CreateShop([FromBody] CreateShopRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Check if user already has a shop
        var existingShop = await GetUserShop(userId.Value);
        if (existingShop != null)
        {
            return BadRequest(new { message = "You already have a shop" });
        }

        // Update user role to seller
        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null) return NotFound();

        var sellerRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.RoleName == "seller");
        if (sellerRole != null)
        {
            user.RoleId = sellerRole.RoleId;
            _unitOfWork.Users.Update(user);
        }

        var shop = new Shop
        {
            SellerId = userId.Value,
            ShopName = request.ShopName,
            Description = request.Description,
            LogoImageUrl = request.LogoImageUrl,
            BannerImageUrl = request.BannerImageUrl,
            IsApproved = false, // Requires admin approval
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Shops.AddAsync(shop);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetShop), MapToShopDto(shop));
    }

    [HttpPut("shop")]
    public async Task<ActionResult<ShopDto>> UpdateShop([FromBody] UpdateShopRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        shop.ShopName = request.ShopName;
        shop.Description = request.Description;
        shop.LogoImageUrl = request.LogoImageUrl;
        shop.BannerImageUrl = request.BannerImageUrl;
        shop.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Shops.Update(shop);
        await _unitOfWork.SaveChangesAsync();

        return Ok(MapToShopDto(shop));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<SellerStatsDto>> GetStats()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var totalProducts = await _context.Products.CountAsync(p => p.ShopId == shop.ShopId);
        var activeProducts = await _context.Products.CountAsync(p => p.ShopId == shop.ShopId && p.IsActive);
        var shopOrders = await _context.ShopOrders.Where(so => so.ShopId == shop.ShopId).ToListAsync();
        var totalOrders = shopOrders.Count;
        var pendingOrders = shopOrders.Count(so => so.ShopOrderStatus.ToLower() == "pending");
        var totalRevenue = shopOrders.Where(so => so.ShopOrderStatus.ToLower() != "cancelled").Sum(so => so.ShopTotal);

        return Ok(new SellerStatsDto
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalRevenue = totalRevenue,
            AverageRating = shop.AverageRating
        });
    }

    #endregion

    #region Product Management

    [HttpGet("products")]
    public async Task<ActionResult<PagedResult<SellerProductDto>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var query = _context.Products
            .Where(p => p.ShopId == shop.ShopId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var productDtos = products.Select(MapToSellerProductDto).ToList();

        return Ok(new PagedResult<SellerProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<SellerProductDto>> GetProduct(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var product = await _context.Products
            .Where(p => p.ProductId == id && p.ShopId == shop.ShopId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(MapToSellerProductDto(product));
    }

    [HttpPost("products")]
    public async Task<ActionResult<SellerProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found. Create a shop first." });
        }

        if (!shop.IsApproved)
        {
            return BadRequest(new { message = "Your shop is not yet approved. Please wait for admin approval." });
        }

        // Verify category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest(new { message = "Invalid category" });
        }

        // Generate slug
        var slug = GenerateSlug(request.ProductName);
        var slugExists = await _unitOfWork.Products.AnyAsync(p => p.Slug == slug);
        if (slugExists)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var product = new Product
        {
            ShopId = shop.ShopId,
            CategoryId = request.CategoryId,
            ProductName = request.ProductName,
            Slug = slug,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            MainImageUrl = request.MainImageUrl,
            Brand = request.Brand,
            Model = request.Model,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Add additional images
        if (request.AdditionalImages?.Any() == true)
        {
            foreach (var imageUrl in request.AdditionalImages)
            {
                var image = new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = imageUrl
                };
                await _unitOfWork.ProductImages.AddAsync(image);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // Reload product with category and images
        var createdProduct = await _context.Products
            .Where(p => p.ProductId == product.ProductId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, MapToSellerProductDto(createdProduct));
    }

    [HttpPut("products/{id:int}")]
    public async Task<ActionResult<SellerProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var product = await _context.Products
            .Where(p => p.ProductId == id && p.ShopId == shop.ShopId)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        // Verify category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest(new { message = "Invalid category" });
        }

        // Update slug if name changed
        if (product.ProductName != request.ProductName)
        {
            var newSlug = GenerateSlug(request.ProductName);
            var slugExists = await _unitOfWork.Products.AnyAsync(p => p.Slug == newSlug && p.ProductId != id);
            if (slugExists)
            {
                newSlug = $"{newSlug}-{DateTime.UtcNow.Ticks}";
            }
            product.Slug = newSlug;
        }

        product.ProductName = request.ProductName;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.MainImageUrl = request.MainImageUrl;
        product.Brand = request.Brand;
        product.Model = request.Model;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        // Update images if provided
        if (request.AdditionalImages != null)
        {
            // Remove old images
            _unitOfWork.ProductImages.RemoveRange(product.Images);

            // Add new images
            foreach (var imageUrl in request.AdditionalImages)
            {
                var image = new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = imageUrl
                };
                await _unitOfWork.ProductImages.AddAsync(image);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Reload product
        var updatedProduct = await _context.Products
            .Where(p => p.ProductId == product.ProductId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstAsync();

        return Ok(MapToSellerProductDto(updatedProduct));
    }

    [HttpDelete("products/{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var product = await _context.Products
            .Where(p => p.ProductId == id && p.ShopId == shop.ShopId)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        // Check if product has any orders
        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (hasOrders)
        {
            // Soft delete - just deactivate
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);
        }
        else
        {
            // Hard delete
            _unitOfWork.Products.Remove(product);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully" });
    }

    #endregion

    #region Order Management

    [HttpGet("orders")]
    public async Task<ActionResult<PagedResult<SellerOrderListDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var query = _context.ShopOrders
            .Where(so => so.ShopId == shop.ShopId)
            .Include(so => so.Order)
                .ThenInclude(o => o.User)
            .Include(so => so.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(so => so.ShopOrderStatus.ToLower() == status.ToLower());
        }

        query = query.OrderByDescending(so => so.Order.OrderDate);

        var totalCount = await query.CountAsync();
        var shopOrders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderDtos = shopOrders.Select(so => new SellerOrderListDto
        {
            ShopOrderId = so.ShopOrderId,
            OrderId = so.OrderId,
            Status = so.ShopOrderStatus,
            TrackingNumber = so.TrackingNumber,
            Subtotal = so.ShopSubtotal,
            Total = so.ShopTotal,
            OrderDate = so.Order.OrderDate,
            ItemCount = so.OrderItems.Sum(oi => oi.Quantity),
            CustomerName = $"{so.Order.User.FirstName} {so.Order.User.LastName}",
            CustomerEmail = so.Order.User.Email
        }).ToList();

        return Ok(new PagedResult<SellerOrderListDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<SellerOrderDetailDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var shopOrder = await _context.ShopOrders
            .Where(so => so.ShopOrderId == id && so.ShopId == shop.ShopId)
            .Include(so => so.Order)
                .ThenInclude(o => o.User)
            .Include(so => so.Order)
                .ThenInclude(o => o.ShippingAddress)
            .Include(so => so.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync();

        if (shopOrder == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(new SellerOrderDetailDto
        {
            ShopOrderId = shopOrder.ShopOrderId,
            OrderId = shopOrder.OrderId,
            Status = shopOrder.ShopOrderStatus,
            TrackingNumber = shopOrder.TrackingNumber,
            Subtotal = shopOrder.ShopSubtotal,
            Total = shopOrder.ShopTotal,
            OrderDate = shopOrder.Order.OrderDate,
            ShippingAddress = new AddressDto
            {
                AddressId = shopOrder.Order.ShippingAddress.AddressId,
                AddressLabel = shopOrder.Order.ShippingAddress.AddressLabel,
                StreetAddress = shopOrder.Order.ShippingAddress.StreetAddress,
                City = shopOrder.Order.ShippingAddress.City,
                PostalCode = shopOrder.Order.ShippingAddress.PostalCode,
                Country = shopOrder.Order.ShippingAddress.Country
            },
            Customer = new CustomerInfoDto
            {
                UserId = shopOrder.Order.User.UserId,
                FirstName = shopOrder.Order.User.FirstName,
                LastName = shopOrder.Order.User.LastName,
                Email = shopOrder.Order.User.Email,
                Phone = shopOrder.Order.User.Phone
            },
            Items = shopOrder.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.ProductName,
                Slug = oi.Product.Slug,
                MainImageUrl = oi.Product.MainImageUrl,
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.PriceAtPurchase
            }).ToList()
        });
    }

    [HttpPut("orders/{id:int}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var shopOrder = await _context.ShopOrders
            .Where(so => so.ShopOrderId == id && so.ShopId == shop.ShopId)
            .FirstOrDefaultAsync();

        if (shopOrder == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        var validStatuses = new[] { "pending", "confirmed", "processing", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(request.Status.ToLower()))
        {
            return BadRequest(new { message = "Invalid status. Valid values: pending, confirmed, processing, shipped, delivered, cancelled" });
        }

        shopOrder.ShopOrderStatus = request.Status.ToLower();
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Order status updated successfully", status = shopOrder.ShopOrderStatus });
    }

    [HttpPut("orders/{id:int}/tracking")]
    public async Task<ActionResult> UpdateTracking(int id, [FromBody] UpdateTrackingRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var shopOrder = await _context.ShopOrders
            .Where(so => so.ShopOrderId == id && so.ShopId == shop.ShopId)
            .FirstOrDefaultAsync();

        if (shopOrder == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        shopOrder.TrackingNumber = request.TrackingNumber;
        if (shopOrder.ShopOrderStatus.ToLower() == "processing")
        {
            shopOrder.ShopOrderStatus = "shipped";
        }
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Tracking number updated successfully", trackingNumber = shopOrder.TrackingNumber });
    }

    #endregion

    #region Discount Management

    [HttpGet("discounts")]
    public async Task<ActionResult<List<SellerDiscountDto>>> GetDiscounts()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var discounts = await _context.Discounts
            .Where(d => d.ShopId == shop.ShopId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Ok(discounts.Select(MapToDiscountDto).ToList());
    }

    [HttpPost("discounts")]
    public async Task<ActionResult<SellerDiscountDto>> CreateDiscount([FromBody] CreateDiscountRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        // Check for duplicate code
        var codeExists = await _context.Discounts.AnyAsync(d => d.Code == request.Code);
        if (codeExists)
        {
            return BadRequest(new { message = "Discount code already exists" });
        }

        var discount = new Discount
        {
            ShopId = shop.ShopId,
            Code = request.Code.ToUpper(),
            DiscountType = request.DiscountType,
            Value = request.Value,
            MinimumOrderAmount = request.MinimumOrderAmount,
            UsageLimit = request.UsageLimit,
            PerUserLimit = request.PerUserLimit,
            ExpiresAt = request.ExpiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Discounts.AddAsync(discount);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDiscounts), MapToDiscountDto(discount));
    }

    [HttpPut("discounts/{id:int}")]
    public async Task<ActionResult<SellerDiscountDto>> UpdateDiscount(int id, [FromBody] UpdateDiscountRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var discount = await _context.Discounts
            .Where(d => d.DiscountId == id && d.ShopId == shop.ShopId)
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return NotFound(new { message = "Discount not found" });
        }

        // Check for duplicate code (exclude current discount)
        var codeExists = await _context.Discounts.AnyAsync(d => d.Code == request.Code && d.DiscountId != id);
        if (codeExists)
        {
            return BadRequest(new { message = "Discount code already exists" });
        }

        discount.Code = request.Code.ToUpper();
        discount.DiscountType = request.DiscountType;
        discount.Value = request.Value;
        discount.MinimumOrderAmount = request.MinimumOrderAmount;
        discount.UsageLimit = request.UsageLimit;
        discount.PerUserLimit = request.PerUserLimit;
        discount.ExpiresAt = request.ExpiresAt;
        discount.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync();

        return Ok(MapToDiscountDto(discount));
    }

    [HttpDelete("discounts/{id:int}")]
    public async Task<ActionResult> DeleteDiscount(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var shop = await GetUserShop(userId.Value);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var discount = await _context.Discounts
            .Where(d => d.DiscountId == id && d.ShopId == shop.ShopId)
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return NotFound(new { message = "Discount not found" });
        }

        _unitOfWork.Discounts.Remove(discount);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Discount deleted successfully" });
    }

    #endregion

    #region Helper Methods

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    private static ShopDto MapToShopDto(Shop shop)
    {
        return new ShopDto
        {
            ShopId = shop.ShopId,
            ShopName = shop.ShopName,
            Description = shop.Description,
            LogoImageUrl = shop.LogoImageUrl,
            BannerImageUrl = shop.BannerImageUrl,
            AverageRating = shop.AverageRating,
            TotalSales = shop.TotalSales,
            IsApproved = shop.IsApproved,
            CreatedAt = shop.CreatedAt
        };
    }

    private static SellerProductDto MapToSellerProductDto(Product product)
    {
        return new SellerProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            MainImageUrl = product.MainImageUrl,
            Brand = product.Brand,
            Model = product.Model,
            AverageRating = product.AverageRating,
            TotalReviews = product.TotalReviews,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.CategoryName,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Images = product.Images.Select(i => i.ImageUrl).ToList()
        };
    }

    private static SellerDiscountDto MapToDiscountDto(Discount discount)
    {
        return new SellerDiscountDto
        {
            DiscountId = discount.DiscountId,
            Code = discount.Code,
            DiscountType = discount.DiscountType,
            Value = discount.Value,
            MinimumOrderAmount = discount.MinimumOrderAmount,
            UsageLimit = discount.UsageLimit,
            UsageCount = discount.UsageCount,
            PerUserLimit = discount.PerUserLimit,
            ExpiresAt = discount.ExpiresAt,
            IsActive = discount.IsActive,
            CreatedAt = discount.CreatedAt
        };
    }

    #endregion
}
