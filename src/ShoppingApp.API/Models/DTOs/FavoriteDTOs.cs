namespace ShoppingApp.API.Models.DTOs;

public class FavoriteItemDto
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
    public DateTime AddedAt { get; set; }
    public bool IsAvailable { get; set; }
}

public class FavoritesDto
{
    public List<FavoriteItemDto> Items { get; set; } = new();
    public int TotalItems => Items.Count;
}
