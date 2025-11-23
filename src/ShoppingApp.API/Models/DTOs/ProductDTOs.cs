namespace ShoppingApp.API.Models.DTOs;

public class ProductListDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ShopSummaryDto Shop { get; set; } = null!;
    public CategoryDto Category { get; set; } = null!;
    public List<string> Images { get; set; } = new();
}

public class ShopSummaryDto
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? LogoImageUrl { get; set; }
    public decimal AverageRating { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}

public class ProductQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Brand { get; set; }
    public string? SortBy { get; set; } // price_asc, price_desc, rating, newest
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
