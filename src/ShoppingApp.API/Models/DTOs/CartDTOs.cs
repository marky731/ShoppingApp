using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.API.Models.DTOs;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? MainImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
    public int StockQuantity { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public int ShopId { get; set; }
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItems => Items.Sum(i => i.Quantity);
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
}

public class AddToCartRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}
