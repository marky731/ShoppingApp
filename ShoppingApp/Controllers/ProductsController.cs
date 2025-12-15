using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Products
        public ActionResult Index(ProductFilterViewModel filters)
        {
            if (filters == null)
            {
                filters = new ProductFilterViewModel();
            }

            var query = db.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Shop.IsApproved);

            // Apply search filter
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(searchTerm)
                    || p.Description.ToLower().Contains(searchTerm)
                    || p.Brand.ToLower().Contains(searchTerm));
            }

            // Apply category filter
            if (filters.CategoryId.HasValue)
            {
                var categoryIds = GetCategoryAndChildIds(filters.CategoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Apply price filters
            if (filters.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filters.MinPrice.Value);
            }
            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filters.MaxPrice.Value);
            }

            // Apply brand filter
            if (!string.IsNullOrEmpty(filters.Brand))
            {
                query = query.Where(p => p.Brand == filters.Brand);
            }

            // Apply sorting
            switch (filters.SortBy)
            {
                case "price_low":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_high":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "rating":
                    query = query.OrderByDescending(p => p.AverageRating);
                    break;
                case "popular":
                    query = query.OrderByDescending(p => p.TotalReviews);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var viewModel = new ProductListViewModel
            {
                Products = query.ToPagedList(filters.Page, filters.PageSize),
                Filters = filters,
                Categories = db.Categories
                    .Where(c => c.ParentCategoryId == null)
                    .Include(c => c.SubCategories)
                    .OrderBy(c => c.CategoryName)
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Products/Details/5 or Products/Details/slug
        public ActionResult Details(string id)
        {
            Product product = null;

            // Try to parse as integer ID first
            int productId;
            if (int.TryParse(id, out productId))
            {
                product = db.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Specifications)
                    .Include(p => p.Reviews.Select(r => r.User))
                    .Include(p => p.Reviews.Select(r => r.Responses))
                    .FirstOrDefault(p => p.ProductId == productId && p.IsActive);
            }

            // If not found by ID, try to find by slug
            if (product == null)
            {
                product = db.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Specifications)
                    .Include(p => p.Reviews.Select(r => r.User))
                    .Include(p => p.Reviews.Select(r => r.Responses))
                    .FirstOrDefault(p => p.Slug == id && p.IsActive);
            }

            if (product == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            var isFavorite = false;
            var canReview = false;
            var cartQuantity = 0;
            var hasPurchased = false;
            var hasDeliveredOrder = false;
            var hasReviewed = false;
            ReviewStatus? userReviewStatus = null;

            if (!string.IsNullOrEmpty(userId))
            {
                isFavorite = db.Favorites.Any(f => f.UserId == userId && f.ProductId == product.ProductId);

                // Check if user has purchased this product
                hasPurchased = db.ShopOrders
                    .Any(so => so.Order.UserId == userId
                        && so.OrderItems.Any(oi => oi.ProductId == product.ProductId));

                // Check if any order with this product is delivered
                hasDeliveredOrder = db.ShopOrders
                    .Any(so => so.Order.UserId == userId
                        && so.ShopOrderStatus == OrderStatus.Delivered
                        && so.OrderItems.Any(oi => oi.ProductId == product.ProductId));

                // Check if user already reviewed this product
                var existingReview = db.Reviews.FirstOrDefault(r => r.UserId == userId && r.ProductId == product.ProductId);
                hasReviewed = existingReview != null;
                userReviewStatus = existingReview?.Status;

                // Can review if has delivered order and hasn't reviewed yet
                canReview = hasDeliveredOrder && !hasReviewed;

                var cartItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == product.ProductId);
                cartQuantity = cartItem?.Quantity ?? 0;
            }

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = db.Products
                    .Include(p => p.Shop)
                    .Where(p => p.CategoryId == product.CategoryId
                        && p.ProductId != product.ProductId
                        && p.IsActive
                        && p.Shop.IsApproved)
                    .OrderByDescending(p => p.AverageRating)
                    .Take(4)
                    .ToList(),
                Reviews = product.Reviews
                    .Where(r => r.Status == ReviewStatus.Approved)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList(),
                IsFavorite = isFavorite,
                CanReview = canReview,
                CartQuantity = cartQuantity,
                HasPurchased = hasPurchased,
                HasDeliveredOrder = hasDeliveredOrder,
                HasReviewed = hasReviewed,
                UserReviewStatus = userReviewStatus
            };

            return View(viewModel);
        }

        // Keep BySlug for backwards compatibility - redirects to Details
        public ActionResult BySlugRedirect(string slug)
        {
            var product = db.Products.FirstOrDefault(p => p.Slug == slug && p.IsActive);
            if (product == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Details", new { id = product.ProductId });
        }

        private int[] GetCategoryAndChildIds(int categoryId)
        {
            var ids = new System.Collections.Generic.List<int> { categoryId };
            var childCategories = db.Categories.Where(c => c.ParentCategoryId == categoryId).ToList();
            foreach (var child in childCategories)
            {
                ids.AddRange(GetCategoryAndChildIds(child.CategoryId));
            }
            return ids.ToArray();
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
