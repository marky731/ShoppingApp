using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.ViewModels
{
    public class SellerDashboardViewModel
    {
        public Shop Shop { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public IEnumerable<DashboardRecentOrderViewModel> RecentOrders { get; set; }
        public IEnumerable<DashboardLowStockViewModel> LowStockProducts { get; set; }
    }

    public class DashboardRecentOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardLowStockViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
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

    // ViewModels expected by Views
    public class ShopViewModel
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Shop Name")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(500)]
        public string Address { get; set; }
    }

    public class SellerProductListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int Stock { get; set; }
        public int TotalSales { get; set; }
        public bool IsActive { get; set; }
    }

    public class SellerProductViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Sku { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(100)]
        public string Brand { get; set; }

        [StringLength(500)]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "Compare At Price")]
        public decimal? CompareAtPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<Category> Categories { get; set; }
    }

    public class SellerOrderListItemViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class SellerOrderDetailsViewModelNew
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public OrderStatus Status { get; set; }
        public IEnumerable<OrderItemViewModel> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountCode { get; set; }
        public decimal Total { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
    }

    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string ImageUrl => ProductImageUrl; // Alias for views
        public string Sku { get; set; }
        public string ProductSku => Sku; // Alias for views
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

    public class DiscountListItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public int UsageCount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinimumPurchase { get; set; }
        public bool IsActive { get; set; }
    }

    public class DiscountViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Discount Code")]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Discount Type")]
        public DiscountType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        [Display(Name = "Minimum Purchase")]
        public decimal? MinimumPurchase { get; set; }

        [Display(Name = "Usage Limit")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public int UsageCount { get; set; }
    }

    // ===== Seller Review ViewModels =====

    public class SellerReviewListItemViewModel
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasResponse { get; set; }
    }

    public class SellerReviewDetailsViewModel
    {
        public Review Review { get; set; }
        public Product Product { get; set; }
        public ApplicationUser Customer { get; set; }
        public ReviewResponse Response { get; set; }
    }
}
