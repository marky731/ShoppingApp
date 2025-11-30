using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.API.Models.DTOs;

public class OrderListDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderShopSummaryDto> Shops { get; set; } = new();
}

public class OrderShopSummaryDto
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class OrderDetailDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DiscountCode { get; set; }
    public AddressDto ShippingAddress { get; set; } = null!;
    public List<ShopOrderDto> ShopOrders { get; set; } = new();
}

public class ShopOrderDto
{
    public int ShopOrderId { get; set; }
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? LogoImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal Subtotal => PriceAtPurchase * Quantity;
}

public class CreateOrderRequest
{
    [Required]
    public int ShippingAddressId { get; set; }

    public string? DiscountCode { get; set; }
}

public class OrderResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? OrderId { get; set; }
    public OrderDetailDto? Order { get; set; }
}
