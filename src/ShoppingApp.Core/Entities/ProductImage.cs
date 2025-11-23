namespace ShoppingApp.Core.Entities;

public class ProductImage
{
    public int ImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation
    public Product Product { get; set; } = null!;
}
