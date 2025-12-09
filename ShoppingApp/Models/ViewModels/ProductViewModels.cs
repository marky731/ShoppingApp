using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PagedList;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public ProductFilterViewModel Filters { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }

    public class ProductFilterViewModel
    {
        [Display(Name = "Search")]
        public string SearchTerm { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Min Price")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Max Price")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Brand")]
        public string Brand { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "newest";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Product> RelatedProducts { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public bool IsFavorite { get; set; }
        public bool CanReview { get; set; }
        public int CartQuantity { get; set; }
    }

    public class ProductCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string ImageUrl { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int DiscountPercentage { get; set; }
        public int Rating { get; set; }
        public int ReviewCount { get; set; }
        public int Stock { get; set; }
        public bool IsFavorite { get; set; }
    }
}
