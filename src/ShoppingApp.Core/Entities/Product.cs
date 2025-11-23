namespace ShoppingApp.Core.Entities;

public class Product
{
    public int ProductId { get; set; }
    public int ShopId { get; set; }
    public int CategoryId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } = 0;
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Shop Shop { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
