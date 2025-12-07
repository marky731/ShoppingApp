using System.Collections.Generic;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Product> FeaturedProducts { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> NewArrivals { get; set; }
    }
}
