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
    public class CartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Cart
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var cartItems = db.CartItems
                .Include(c => c.Product.Shop)
                .Where(c => c.UserId == userId)
                .ToList();

            var viewModel = new CartViewModel
            {
                Items = cartItems.Select(c => new CartItemViewModel
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product.ProductName,
                    ProductImage = c.Product.MainImageUrl,
                    ShopName = c.Product.Shop.ShopName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    StockQuantity = c.Product.StockQuantity,
                    ItemTotal = c.Product.Price * c.Quantity
                }).ToList(),
                Subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity),
                TotalItems = cartItems.Sum(c => c.Quantity)
            };

            return View(viewModel);
        }

        // POST: Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddToCartViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            var userId = User.Identity.GetUserId();
            var product = db.Products.Include(p => p.Shop).FirstOrDefault(p => p.ProductId == model.ProductId);

            if (product == null || !product.IsActive || !product.Shop.IsApproved)
            {
                TempData["Error"] = "Product not available.";
                return RedirectToAction("Index", "Products");
            }

            if (model.Quantity > product.StockQuantity)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            var existingCartItem = db.CartItems
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == model.ProductId);

            if (existingCartItem != null)
            {
                var newQuantity = existingCartItem.Quantity + model.Quantity;
                if (newQuantity > product.StockQuantity)
                {
                    TempData["Error"] = "Cannot add more than available stock.";
                    return RedirectToAction("Details", "Products", new { id = model.ProductId });
                }
                existingCartItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();
            TempData["Success"] = "Product added to cart.";

            return RedirectToAction("Index");
        }

        // POST: Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(UpdateCartItemViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var cartItem = db.CartItems
                .Include(c => c.Product)
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == model.ProductId);

            if (cartItem == null)
            {
                TempData["Error"] = "Item not found in cart.";
                return RedirectToAction("Index");
            }

            if (model.Quantity > cartItem.Product.StockQuantity)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Index");
            }

            cartItem.Quantity = model.Quantity;
            db.SaveChanges();

            TempData["Success"] = "Cart updated.";
            return RedirectToAction("Index");
        }

        // POST: Cart/Remove/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var userId = User.Identity.GetUserId();
            var cartItem = db.CartItems
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == id);

            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        // POST: Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clear()
        {
            var userId = User.Identity.GetUserId();
            var cartItems = db.CartItems.Where(c => c.UserId == userId).ToList();

            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Success"] = "Cart cleared.";
            return RedirectToAction("Index");
        }

        // POST: Cart/AddJson (AJAX - for favorites page)
        [HttpPost]
        public JsonResult AddJson(int productId, int quantity = 1)
        {
            var userId = User.Identity.GetUserId();
            var product = db.Products.Include(p => p.Shop).FirstOrDefault(p => p.ProductId == productId);

            if (product == null || !product.IsActive || !product.Shop.IsApproved)
            {
                return Json(new { success = false, message = "Product not available." });
            }

            if (quantity > product.StockQuantity)
            {
                return Json(new { success = false, message = "Not enough stock available." });
            }

            var existingCartItem = db.CartItems
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (existingCartItem != null)
            {
                var newQuantity = existingCartItem.Quantity + quantity;
                if (newQuantity > product.StockQuantity)
                {
                    return Json(new { success = false, message = "Cannot add more than available stock." });
                }
                existingCartItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                };
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();

            var cartCount = db.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => (int?)c.Quantity) ?? 0;

            return Json(new { success = true, message = "Product added to cart.", cartCount = cartCount });
        }

        // GET: Cart/Count (AJAX)
        public JsonResult Count()
        {
            var userId = User.Identity.GetUserId();
            var count = db.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => (int?)c.Quantity) ?? 0;

            return Json(new { count }, JsonRequestBehavior.AllowGet);
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
