namespace ShoppingApp.Core.Entities;

public class Review
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public ReviewResponse? Response { get; set; }
}
