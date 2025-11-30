using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.API.Models.DTOs;

// Shop DTOs
public class ShopDto
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoImageUrl { get; set; }
    public string? BannerImageUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalSales { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateShopRequest
{
    [Required]
    [StringLength(255)]
    public string ShopName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? LogoImageUrl { get; set; }

    [StringLength(255)]
    public string? BannerImageUrl { get; set; }
}

public class CreateShopRequest
{
    [Required]
    [StringLength(255)]
    public string ShopName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? LogoImageUrl { get; set; }

    [StringLength(255)]
    public string? BannerImageUrl { get; set; }
}

// Seller Product DTOs
public class SellerProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Images { get; set; } = new();
    public List<ProductSpecificationDto> Specifications { get; set; } = new();
}

public class CreateProductRequest
{
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    [Required]
    public int CategoryId { get; set; }

    [StringLength(255)]
    public string? MainImageUrl { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    public List<string>? AdditionalImages { get; set; }

    public List<SpecificationInput>? Specifications { get; set; }
}

public class SpecificationInput
{
    [Required]
    [StringLength(100)]
    public string SpecName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string SpecValue { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;
}

public class UpdateProductRequest
{
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [StringLength(255)]
    public string? MainImageUrl { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string>? AdditionalImages { get; set; }

    public List<SpecificationInput>? Specifications { get; set; }
}

// Seller Order DTOs
public class SellerOrderListDto
{
    public int ShopOrderId { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

public class SellerOrderDetailDto
{
    public int ShopOrderId { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public AddressDto ShippingAddress { get; set; } = null!;
    public CustomerInfoDto Customer { get; set; } = null!;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class CustomerInfoDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty; // pending, confirmed, processing, shipped, delivered, cancelled
}

public class UpdateTrackingRequest
{
    [Required]
    [StringLength(100)]
    public string TrackingNumber { get; set; } = string.Empty;
}

// Seller Discount DTOs
public class SellerDiscountDto
{
    public int DiscountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public int PerUserLimit { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDiscountRequest
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(percentage|fixed_amount)$", ErrorMessage = "DiscountType must be 'percentage' or 'fixed_amount'")]
    public string DiscountType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Value { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumOrderAmount { get; set; } = 0;

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int PerUserLimit { get; set; } = 1;

    public DateTime? ExpiresAt { get; set; }
}

public class UpdateDiscountRequest
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(percentage|fixed_amount)$", ErrorMessage = "DiscountType must be 'percentage' or 'fixed_amount'")]
    public string DiscountType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Value { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumOrderAmount { get; set; } = 0;

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int PerUserLimit { get; set; } = 1;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}

// Seller Stats DTO
public class SellerStatsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
}
