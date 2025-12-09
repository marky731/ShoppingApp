using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Controllers
{
    public class ShopsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private const int PageSize = 12;

        // GET: Shops/Details/5
        public ActionResult Details(int id, int page = 1)
        {
            var shop = db.Shops
                .Include(s => s.Seller)
                .FirstOrDefault(s => s.ShopId == id && s.IsApproved);

            if (shop == null)
            {
                return HttpNotFound();
            }

            var productsQuery = db.Products
                .Include(p => p.Category)
                .Include(p => p.Shop)
                .Where(p => p.ShopId == id && p.IsActive)
                .OrderByDescending(p => p.CreatedAt);

            var totalProducts = productsQuery.Count();
            var totalPages = (totalProducts + PageSize - 1) / PageSize;
            var products = productsQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var viewModel = new ShopDetailsViewModel
            {
                ShopId = shop.ShopId,
                Name = shop.ShopName,
                Description = shop.Description,
                LogoUrl = shop.LogoImageUrl,
                Slug = shop.ShopId.ToString(),
                Rating = shop.AverageRating,
                ReviewCount = 0,
                ProductCount = totalProducts,
                TotalSales = shop.TotalSales,
                CreatedAt = shop.CreatedAt,
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages
            };

            // Set ViewBag for filters
            ViewBag.Categories = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName).ToList(),
                "CategoryId",
                "CategoryName"
            );

            return View(viewModel);
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
