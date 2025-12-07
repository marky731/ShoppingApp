namespace ShoppingApp.Models.Domain
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    public enum ReviewStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
