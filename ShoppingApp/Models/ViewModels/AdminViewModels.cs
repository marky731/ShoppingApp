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
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingSellers { get; set; }
        public int PendingReviews { get; set; }
        public IEnumerable<Order> RecentOrders { get; set; }
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
}
