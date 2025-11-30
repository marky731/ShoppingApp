using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Favorites;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public FavoritesController(IUnitOfWork unitOfWork, AppDbContext context)
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
    public async Task<ActionResult<FavoritesDto>> GetFavorites()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
                .ThenInclude(p => p.Shop)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var favoritesDto = new FavoritesDto
        {
            Items = favorites.Select(f => new FavoriteItemDto
            {
                ProductId = f.ProductId,
                ProductName = f.Product.ProductName,
                Slug = f.Product.Slug,
                Price = f.Product.Price,
                MainImageUrl = f.Product.MainImageUrl,
                Brand = f.Product.Brand,
                AverageRating = f.Product.AverageRating,
                TotalReviews = f.Product.TotalReviews,
                ShopName = f.Product.Shop.ShopName,
                AddedAt = f.CreatedAt,
                IsAvailable = f.Product.IsActive && f.Product.StockQuantity > 0 && f.Product.Shop.IsApproved
            }).ToList()
        };

        return Ok(favoritesDto);
    }

    [HttpPost("{productId:int}")]
    public async Task<ActionResult> AddToFavorites(int productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Check if product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        // Check if already in favorites
        var existingFavorite = await _unitOfWork.Favorites.FirstOrDefaultAsync(
            f => f.UserId == userId && f.ProductId == productId);

        if (existingFavorite != null)
        {
            return BadRequest(new { message = "Product already in favorites" });
        }

        var favorite = new Favorite
        {
            UserId = userId.Value,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Favorites.AddAsync(favorite);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Added to favorites" });
    }

    [HttpDelete("{productId:int}")]
    public async Task<ActionResult> RemoveFromFavorites(int productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var favorite = await _unitOfWork.Favorites.FirstOrDefaultAsync(
            f => f.UserId == userId && f.ProductId == productId);

        if (favorite == null)
        {
            return NotFound(new { message = "Product not found in favorites" });
        }

        _unitOfWork.Favorites.Remove(favorite);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Removed from favorites" });
    }

    [HttpGet("check/{productId:int}")]
    public async Task<ActionResult> CheckFavorite(int productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var isFavorite = await _unitOfWork.Favorites.AnyAsync(
            f => f.UserId == userId && f.ProductId == productId);

        return Ok(new { isFavorite });
    }
}
