using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using PagedList;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class SellerDashboardViewModel
    {
        public Shop Shop { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public IEnumerable<ShopOrder> RecentOrders { get; set; }
    }

    public class CreateShopViewModel
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Logo Image URL")]
        public string LogoImageUrl { get; set; }

        [StringLength(255)]
        [Display(Name = "Banner Image URL")]
        public string BannerImageUrl { get; set; }
    }

    public class EditShopViewModel
    {
        public int ShopId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Logo Image URL")]
        public string LogoImageUrl { get; set; }

        [StringLength(255)]
        [Display(Name = "Banner Image URL")]
        public string BannerImageUrl { get; set; }
    }

    public class SellerProductListViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateProductViewModel
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [StringLength(255)]
        [Display(Name = "Main Image URL")]
        public string MainImageUrl { get; set; }

        [StringLength(100)]
        public string Brand { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        public IEnumerable<Category> Categories { get; set; }
        public List<ProductSpecificationViewModel> Specifications { get; set; } = new List<ProductSpecificationViewModel>();
        public List<string> AdditionalImageUrls { get; set; } = new List<string>();
    }

    public class EditProductViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [StringLength(255)]
        [Display(Name = "Main Image URL")]
        public string MainImageUrl { get; set; }

        [StringLength(100)]
        public string Brand { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public IEnumerable<Category> Categories { get; set; }
        public List<ProductSpecificationViewModel> Specifications { get; set; } = new List<ProductSpecificationViewModel>();
        public List<ProductImage> ExistingImages { get; set; } = new List<ProductImage>();
        public List<string> AdditionalImageUrls { get; set; } = new List<string>();
    }

    public class ProductSpecificationViewModel
    {
        public int? SpecificationId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string SpecName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Value")]
        public string SpecValue { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class SellerOrderListViewModel
    {
        public IPagedList<ShopOrder> Orders { get; set; }
        public OrderStatus? StatusFilter { get; set; }
    }

    public class SellerOrderDetailsViewModel
    {
        public ShopOrder ShopOrder { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }

    public class UpdateOrderStatusViewModel
    {
        public int ShopOrderId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public OrderStatus Status { get; set; }

        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }
    }

    public class DiscountListViewModel
    {
        public IPagedList<Discount> Discounts { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateDiscountViewModel
    {
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

        [Display(Name = "Per User Limit")]
        [Range(1, int.MaxValue)]
        public int PerUserLimit { get; set; } = 1;

        [Display(Name = "Expires At")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAt { get; set; }
    }

    public class EditDiscountViewModel
    {
        public int DiscountId { get; set; }

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

        [Display(Name = "Per User Limit")]
        [Range(1, int.MaxValue)]
        public int PerUserLimit { get; set; } = 1;

        [Display(Name = "Expires At")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public int UsageCount { get; set; }
    }
}
