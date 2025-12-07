using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

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
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var products = favorites.Select(f => f.Product).ToList();

            return View(products);
        }

        // POST: Favorites/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Toggle(int productId, string returnUrl)
        {
            var userId = User.Identity.GetUserId();

            var existing = db.Favorites
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (existing != null)
            {
                db.Favorites.Remove(existing);
                TempData["Success"] = "Product removed from favorites.";
            }
            else
            {
                var product = db.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);
                if (product != null)
                {
                    var favorite = new Favorite
                    {
                        UserId = userId,
                        ProductId = productId,
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
