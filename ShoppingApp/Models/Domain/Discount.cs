using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingApp.Models.Domain
{
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Display(Name = "Shop")]
        public int? ShopId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Discount Code")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "Discount Type")]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        public decimal Value { get; set; }

        [Display(Name = "Minimum Order Amount")]
        [Range(0, double.MaxValue)]
        public decimal MinimumOrderAmount { get; set; } = 0;

        [Display(Name = "Usage Limit")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Times Used")]
        public int UsageCount { get; set; } = 0;

        [Display(Name = "Per User Limit")]
        [Range(1, int.MaxValue)]
        public int PerUserLimit { get; set; } = 1;

        [Display(Name = "Expires At")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("ShopId")]
        public virtual Shop Shop { get; set; }
    }
}
