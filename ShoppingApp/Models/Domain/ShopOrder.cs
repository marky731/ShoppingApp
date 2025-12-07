using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingApp.Models.Domain
{
    public class ShopOrder
    {
        [Key]
        public int ShopOrderId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public OrderStatus ShopOrderStatus { get; set; } = OrderStatus.Pending;

        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }

        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal ShopSubtotal { get; set; }

        [Display(Name = "Discount")]
        public int? DiscountId { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal ShopTotal { get; set; }

        // Navigation
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("ShopId")]
        public virtual Shop Shop { get; set; }

        [ForeignKey("DiscountId")]
        public virtual Discount Discount { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
