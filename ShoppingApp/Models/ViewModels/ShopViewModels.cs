using System;
using System.Collections.Generic;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class ShopCardViewModel
    {
        public int ShopId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public decimal Rating { get; set; }
        public int ProductCount { get; set; }
    }

    public class ShopDetailsViewModel
    {
        public int ShopId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Slug { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int ProductCount { get; set; }
        public int TotalSales { get; set; }
        public DateTime CreatedAt { get; set; }

        public IEnumerable<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
