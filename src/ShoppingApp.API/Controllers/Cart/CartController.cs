using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Cart;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public CartController(IUnitOfWork unitOfWork, AppDbContext context)
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
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
                .ThenInclude(p => p.Shop)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var cartDto = new CartDto
        {
            Items = cartItems.Select(c => new CartItemDto
            {
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                Slug = c.Product.Slug,
                Price = c.Product.Price,
                MainImageUrl = c.Product.MainImageUrl,
                Quantity = c.Quantity,
                StockQuantity = c.Product.StockQuantity,
                ShopName = c.Product.Shop.ShopName,
                ShopId = c.Product.ShopId
            }).ToList()
        };

        return Ok(cartDto);
    }

    [HttpPost]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Check if product exists and is active
        var product = await _context.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && p.IsActive && p.Shop.IsApproved);

        if (product == null)
        {
            return NotFound(new { message = "Product not found or unavailable" });
        }

        // Check stock
        if (product.StockQuantity < request.Quantity)
        {
            return BadRequest(new { message = "Insufficient stock", availableStock = product.StockQuantity });
        }

        // Check if item already in cart
        var existingItem = await _unitOfWork.CartItems.FirstOrDefaultAsync(
            c => c.UserId == userId && c.ProductId == request.ProductId);

        if (existingItem != null)
        {
            // Update quantity
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (newQuantity > product.StockQuantity)
            {
                return BadRequest(new { message = "Insufficient stock", availableStock = product.StockQuantity });
            }
            existingItem.Quantity = newQuantity;
            _unitOfWork.CartItems.Update(existingItem);
        }
        else
        {
            // Add new item
            var cartItem = new CartItem
            {
                UserId = userId.Value,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartItems.AddAsync(cartItem);
        }

        await _unitOfWork.SaveChangesAsync();

        // Return updated cart
        return await GetCart();
    }

    [HttpPut("{productId:int}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(int productId, [FromBody] UpdateCartItemRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cartItem = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (cartItem == null)
        {
            return NotFound(new { message = "Item not found in cart" });
        }

        // Check stock
        if (request.Quantity > cartItem.Product.StockQuantity)
        {
            return BadRequest(new { message = "Insufficient stock", availableStock = cartItem.Product.StockQuantity });
        }

        cartItem.Quantity = request.Quantity;
        _unitOfWork.CartItems.Update(cartItem);
        await _unitOfWork.SaveChangesAsync();

        return await GetCart();
    }

    [HttpDelete("{productId:int}")]
    public async Task<ActionResult<CartDto>> RemoveFromCart(int productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cartItem = await _unitOfWork.CartItems.FirstOrDefaultAsync(
            c => c.UserId == userId && c.ProductId == productId);

        if (cartItem == null)
        {
            return NotFound(new { message = "Item not found in cart" });
        }

        _unitOfWork.CartItems.Remove(cartItem);
        await _unitOfWork.SaveChangesAsync();

        return await GetCart();
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cartItems = await _unitOfWork.CartItems.FindAsync(c => c.UserId == userId);
        _unitOfWork.CartItems.RemoveRange(cartItems);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Cart cleared successfully" });
    }
}
