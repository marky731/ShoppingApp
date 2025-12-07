using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var viewModel = new HomeViewModel
            {
                // Featured products (top rated from approved shops)
                FeaturedProducts = db.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.Shop.IsApproved)
                    .OrderByDescending(p => p.AverageRating)
                    .ThenByDescending(p => p.TotalReviews)
                    .Take(8)
                    .ToList(),

                // Top-level categories
                Categories = db.Categories
                    .Where(c => c.ParentCategoryId == null)
                    .Include(c => c.SubCategories)
                    .OrderBy(c => c.CategoryName)
                    .ToList(),

                // New arrivals
                NewArrivals = db.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.Shop.IsApproved)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToList()
            };

            return View(viewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "About our shopping application.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact us.";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
