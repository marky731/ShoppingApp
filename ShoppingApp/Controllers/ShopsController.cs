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

        // GET: Shops
        public ActionResult Index()
        {
            var shops = db.Shops
                .Where(s => s.IsApproved)
                .OrderByDescending(s => s.AverageRating)
                .ThenByDescending(s => s.TotalSales)
                .Select(s => new ShopCardViewModel
                {
                    ShopId = s.ShopId,
                    Name = s.ShopName,
                    Description = s.Description,
                    LogoUrl = s.LogoImageUrl,
                    Rating = s.AverageRating,
                    ProductCount = db.Products.Count(p => p.ShopId == s.ShopId && p.IsActive)
                })
                .ToList();

            return View(shops);
        }

        // GET: Shops/Details/5
        public ActionResult Details(int id, string search, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortBy, int page = 1)
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
                .Where(p => p.ShopId == id && p.IsActive);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply price range filters
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // Apply sorting
            switch (sortBy)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                case "rating":
                    productsQuery = productsQuery.OrderByDescending(p => p.AverageRating);
                    break;
                default:
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
            }

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
                "CategoryName",
                categoryId
            );
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;

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
