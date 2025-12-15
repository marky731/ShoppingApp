using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalShops { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingSellers { get; set; }
        public int PendingReviews { get; set; }
        public IEnumerable<Order> RecentOrders { get; set; }
        public IEnumerable<ApplicationUser> RecentUsers { get; set; }
    }

    public class UserListViewModel
    {
        public IPagedList<ApplicationUser> Users { get; set; }
        public string SearchTerm { get; set; }
        public string RoleFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
    }

    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class PendingSellersViewModel
    {
        public IPagedList<Shop> PendingShops { get; set; }
    }

    public class CategoryListViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        public IEnumerable<Category> ParentCategories { get; set; }
    }

    public class EditCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        public IEnumerable<Category> ParentCategories { get; set; }
    }

    public class PendingReviewsViewModel
    {
        public IPagedList<Review> PendingReviews { get; set; }
    }

    public class ReviewModerationViewModel
    {
        public Review Review { get; set; }
        public Product Product { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class AdminUserListItemViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class PendingSellerViewModel
    {
        public int Id { get; set; }
        public string ShopName { get; set; }
        public string LogoUrl { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public int ProductCount { get; set; }
        public int SubCategoryCount { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool CanDelete => ProductCount == 0 && SubCategoryCount == 0;
    }

    public class CategoryViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentId { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<Category> ParentCategories { get; set; }
    }

    public class PendingReviewViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
