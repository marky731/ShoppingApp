namespace ShoppingApp.Core.Entities;

public class Shop
{
    public int ShopId { get; set; }
    public int SellerId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoImageUrl { get; set; }
    public string? BannerImageUrl { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int TotalSales { get; set; } = 0;
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Seller { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}
