namespace ShoppingApp.Core.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public int ShippingAddressId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal SubtotalAmount { get; set; }
    public int? DiscountId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Address ShippingAddress { get; set; } = null!;
    public Discount? Discount { get; set; }
    public ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
