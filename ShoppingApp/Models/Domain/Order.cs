using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.Domain
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Shipping Address")]
        public int ShippingAddressId { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal SubtotalAmount { get; set; }

        [Display(Name = "Discount")]
        public int? DiscountId { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [StringLength(100)]
        [Display(Name = "Payment ID")]
        public string PaymentId { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("ShippingAddressId")]
        public virtual Address ShippingAddress { get; set; }

        [ForeignKey("DiscountId")]
        public virtual Discount Discount { get; set; }

        public virtual ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
