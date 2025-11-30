namespace ShoppingApp.API.Models.DTOs;

public class PublicShopDto
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoImageUrl { get; set; }
    public string? BannerImageUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalSales { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductCount { get; set; }
}

public class ShopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public decimal AverageRating { get; set; }
    public int StockQuantity { get; set; }
}

public class ShopDetailDto
{
    public PublicShopDto Shop { get; set; } = null!;
    public List<ShopProductDto> Products { get; set; } = new();
    public int TotalProducts { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
