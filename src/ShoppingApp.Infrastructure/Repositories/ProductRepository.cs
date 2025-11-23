using Microsoft.EntityFrameworkCore;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        int page,
        int pageSize,
        string? search = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? brand = null,
        string? sortBy = null)
    {
        var query = _dbSet
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Shop.IsApproved)
            .AsQueryable();

        // Search
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.ProductName.ToLower().Contains(searchLower) ||
                p.Description.ToLower().Contains(searchLower) ||
                (p.Brand != null && p.Brand.ToLower().Contains(searchLower)));
        }

        // Filters - include products from subcategories
        if (categoryId.HasValue)
            query = query.Where(p =>
                p.CategoryId == categoryId.Value ||
                p.Category.ParentCategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(p => p.Brand == brand);

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<Product?> GetProductWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.ProductId == id && p.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<Product?> GetProductBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.Slug == slug && p.IsActive)
            .FirstOrDefaultAsync();
    }
}
