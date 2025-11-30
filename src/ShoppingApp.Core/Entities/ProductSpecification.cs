namespace ShoppingApp.Core.Entities;

public class ProductSpecification
{
    public int SpecificationId { get; set; }
    public int ProductId { get; set; }
    public string SpecName { get; set; } = string.Empty;
    public string SpecValue { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Product Product { get; set; } = null!;
}
