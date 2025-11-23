namespace ShoppingApp.Core.Entities;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int ShopOrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }

    // Navigation
    public ShopOrder ShopOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
