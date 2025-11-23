namespace ShoppingApp.Core.Entities;

public class ReviewResponse
{
    public int ResponseId { get; set; }
    public int ReviewId { get; set; }
    public int SellerId { get; set; }
    public string ResponseText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Review Review { get; set; } = null!;
    public User Seller { get; set; } = null!;
}
