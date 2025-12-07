using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class CartViewModel
    {
        public IEnumerable<CartItemViewModel> Items { get; set; }
        public decimal Subtotal { get; set; }
        public int TotalItems { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal ItemTotal { get; set; }
    }

    public class AddToCartViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
