using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Favorites
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var favorites = db.Favorites
                .Include(f => f.Product.Shop)
                .Include(f => f.Product.Category)
                .Include(f => f.Product.Reviews)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var productCards = favorites.Select(f => new ProductCardViewModel
            {
                Id = f.Product.ProductId,
                Name = f.Product.ProductName,
                Slug = f.Product.Slug,
                ImageUrl = f.Product.MainImageUrl,
                ShopName = f.Product.Shop?.ShopName ?? "Unknown",
                Price = f.Product.Price,
                OriginalPrice = f.Product.Price, // No discount field in Product model
                DiscountPercentage = 0,
                Rating = f.Product.Reviews.Any() ? (int)Math.Round(f.Product.Reviews.Average(r => r.Rating)) : 0,
                ReviewCount = f.Product.Reviews.Count(),
                Stock = f.Product.StockQuantity,
                IsFavorite = true
            }).ToList();

            return View(productCards);
        }

        // POST: Favorites/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Toggle(int? id, int? productId, string returnUrl)
        {
            // Support both 'id' and 'productId' parameter names
            var actualId = id ?? productId;

            if (!actualId.HasValue)
            {
                TempData["Error"] = "Product ID is required.";
                return RedirectToAction("Index");
            }

            var userId = User.Identity.GetUserId();

            var existing = db.Favorites
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == actualId.Value);

            if (existing != null)
            {
                db.Favorites.Remove(existing);
                TempData["Success"] = "Product removed from favorites.";
            }
            else
            {
                var product = db.Products.FirstOrDefault(p => p.ProductId == actualId.Value && p.IsActive);
                if (product != null)
                {
                    var favorite = new Favorite
                    {
                        UserId = userId,
                        ProductId = actualId.Value,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Favorites.Add(favorite);
                    TempData["Success"] = "Product added to favorites.";
                }
            }

            db.SaveChanges();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        // POST: Favorites/Add (AJAX)
        [HttpPost]
        public JsonResult Add(int productId)
        {
            var userId = User.Identity.GetUserId();

            var existing = db.Favorites
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (existing != null)
            {
                return Json(new { success = false, message = "Product already in favorites." });
            }

            var product = db.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };
            db.Favorites.Add(favorite);
            db.SaveChanges();

            return Json(new { success = true, message = "Product added to favorites." });
        }

        // POST: Favorites/Remove (AJAX)
        [HttpPost]
        public JsonResult Remove(int productId)
        {
            var userId = User.Identity.GetUserId();

            var favorite = db.Favorites
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (favorite == null)
            {
                return Json(new { success = false, message = "Product not in favorites." });
            }

            db.Favorites.Remove(favorite);
            db.SaveChanges();

            return Json(new { success = true, message = "Product removed from favorites." });
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
