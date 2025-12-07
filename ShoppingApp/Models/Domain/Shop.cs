using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.Domain
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        [Required]
        public string SellerId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Logo URL")]
        public string LogoImageUrl { get; set; }

        [StringLength(255)]
        [Display(Name = "Banner URL")]
        public string BannerImageUrl { get; set; }

        [Display(Name = "Average Rating")]
        public decimal AverageRating { get; set; } = 0;

        [Display(Name = "Total Sales")]
        public int TotalSales { get; set; } = 0;

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("SellerId")]
        public virtual ApplicationUser Seller { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}
