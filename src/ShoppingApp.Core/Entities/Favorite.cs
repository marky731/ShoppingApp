namespace ShoppingApp.Core.Entities;

public class Favorite
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
