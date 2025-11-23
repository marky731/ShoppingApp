using ShoppingApp.Core.Entities;

namespace ShoppingApp.Core.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        int page,
        int pageSize,
        string? search = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? brand = null,
        string? sortBy = null);

    Task<Product?> GetProductWithDetailsAsync(int id);
    Task<Product?> GetProductBySlugAsync(string slug);
}
