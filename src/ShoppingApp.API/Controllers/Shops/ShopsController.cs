using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Shops;

[ApiController]
[Route("api/[controller]")]
public class ShopsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ShopsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all approved shops
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PublicShopDto>>> GetShops()
    {
        var shops = await _context.Shops
            .Where(s => s.IsApproved)
            .Select(s => new PublicShopDto
            {
                ShopId = s.ShopId,
                ShopName = s.ShopName,
                Description = s.Description,
                LogoImageUrl = s.LogoImageUrl,
                BannerImageUrl = s.BannerImageUrl,
                AverageRating = s.AverageRating,
                TotalSales = s.TotalSales,
                CreatedAt = s.CreatedAt,
                ProductCount = s.Products.Count(p => p.IsActive)
            })
            .ToListAsync();

        return Ok(shops);
    }

    /// <summary>
    /// Get shop details with products
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShopDetailDto>> GetShop(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var shop = await _context.Shops
            .Where(s => s.ShopId == id && s.IsApproved)
            .Select(s => new PublicShopDto
            {
                ShopId = s.ShopId,
                ShopName = s.ShopName,
                Description = s.Description,
                LogoImageUrl = s.LogoImageUrl,
                BannerImageUrl = s.BannerImageUrl,
                AverageRating = s.AverageRating,
                TotalSales = s.TotalSales,
                CreatedAt = s.CreatedAt,
                ProductCount = s.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

        if (shop == null)
        {
            return NotFound(new { message = "Shop not found" });
        }

        var totalProducts = await _context.Products
            .Where(p => p.ShopId == id && p.IsActive)
            .CountAsync();

        var products = await _context.Products
            .Where(p => p.ShopId == id && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ShopProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Slug = p.Slug,
                Price = p.Price,
                MainImageUrl = p.MainImageUrl,
                Brand = p.Brand,
                AverageRating = p.AverageRating,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();

        return Ok(new ShopDetailDto
        {
            Shop = shop,
            Products = products,
            TotalProducts = totalProducts,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize)
        });
    }
}
