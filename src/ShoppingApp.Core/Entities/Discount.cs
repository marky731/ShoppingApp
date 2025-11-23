namespace ShoppingApp.Core.Entities;

public class Discount
{
    public int DiscountId { get; set; }
    public int? ShopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty; // 'percentage' or 'fixed_amount'
    public decimal Value { get; set; }
    public decimal MinimumOrderAmount { get; set; } = 0;
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; } = 0;
    public int PerUserLimit { get; set; } = 1;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Shop? Shop { get; set; }
}
