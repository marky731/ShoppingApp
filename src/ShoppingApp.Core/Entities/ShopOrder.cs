namespace ShoppingApp.Core.Entities;

public class ShopOrder
{
    public int ShopOrderId { get; set; }
    public int OrderId { get; set; }
    public int ShopId { get; set; }
    public string ShopOrderStatus { get; set; } = "pending";
    public string? TrackingNumber { get; set; }
    public decimal ShopSubtotal { get; set; }
    public int? DiscountId { get; set; }
    public decimal ShopTotal { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Shop Shop { get; set; } = null!;
    public Discount? Discount { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
