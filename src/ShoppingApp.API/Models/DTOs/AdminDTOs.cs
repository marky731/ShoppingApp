using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.API.Models.DTOs;

// User Management DTOs
public class AdminUserDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ShopBasicDto? Shop { get; set; }
}

public class ShopBasicDto
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}

// Seller Application DTOs
public class PendingSellerDto
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public SellerInfoDto Seller { get; set; } = null!;
}

public class SellerInfoDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

// Category Management DTOs
public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }
}

// Review Moderation DTOs
public class PendingReviewDto
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public ReviewerDto Reviewer { get; set; } = null!;
}

public class ReviewerDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Dashboard Stats
public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalSellers { get; set; }
    public int PendingSellers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingReviews { get; set; }
}
