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
    public class CheckoutController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Checkout
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var cartItems = db.CartItems
                .Include(c => c.Product.Shop)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var addresses = db.Addresses.Where(a => a.UserId == userId).ToList();

            var cartViewModel = new CartViewModel
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

            var viewModel = new CheckoutViewModel
            {
                Cart = cartViewModel,
                Addresses = addresses,
                Total = cartViewModel.Subtotal
            };

            return View(viewModel);
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            var userId = User.Identity.GetUserId();

            // Verify address belongs to user
            var address = db.Addresses.FirstOrDefault(a => a.AddressId == model.SelectedAddressId && a.UserId == userId);
            if (address == null)
            {
                TempData["Error"] = "Please select a valid shipping address.";
                return RedirectToAction("Index");
            }

            var cartItems = db.CartItems
                .Include(c => c.Product.Shop)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate stock availability
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.StockQuantity)
                {
                    TempData["Error"] = $"Not enough stock for {item.Product.ProductName}.";
                    return RedirectToAction("Index");
                }
            }

            // Calculate totals
            var subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity);
            var discountAmount = 0m;
            Discount discount = null;

            // Apply discount if provided
            if (!string.IsNullOrEmpty(model.DiscountCode))
            {
                discount = db.Discounts.FirstOrDefault(d =>
                    d.Code.ToUpper() == model.DiscountCode.ToUpper()
                    && d.IsActive
                    && (!d.ExpiresAt.HasValue || d.ExpiresAt > DateTime.UtcNow)
                    && (!d.UsageLimit.HasValue || d.UsageCount < d.UsageLimit)
                    && d.MinimumOrderAmount <= subtotal);

                if (discount != null)
                {
                    discountAmount = discount.DiscountType == DiscountType.Percentage
                        ? subtotal * (discount.Value / 100)
                        : discount.Value;

                    if (discountAmount > subtotal)
                    {
                        discountAmount = subtotal;
                    }
                }
            }

            var total = subtotal - discountAmount;

            // Create main order
            var order = new Order
            {
                UserId = userId,
                ShippingAddressId = model.SelectedAddressId,
                OrderDate = DateTime.UtcNow,
                SubtotalAmount = subtotal,
                DiscountId = discount?.DiscountId,
                TotalAmount = total
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // Group cart items by shop and create shop orders
            var itemsByShop = cartItems.GroupBy(c => c.Product.ShopId);

            foreach (var shopGroup in itemsByShop)
            {
                var shopSubtotal = shopGroup.Sum(c => c.Product.Price * c.Quantity);

                var shopOrder = new ShopOrder
                {
                    OrderId = order.OrderId,
                    ShopId = shopGroup.Key,
                    ShopOrderStatus = OrderStatus.Pending,
                    ShopSubtotal = shopSubtotal,
                    ShopTotal = shopSubtotal
                };

                db.ShopOrders.Add(shopOrder);
                db.SaveChanges();

                // Create order items and update stock
                foreach (var cartItem in shopGroup)
                {
                    var orderItem = new OrderItem
                    {
                        ShopOrderId = shopOrder.ShopOrderId,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtPurchase = cartItem.Product.Price
                    };

                    db.OrderItems.Add(orderItem);

                    // Update stock
                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                }
            }

            // Update discount usage
            if (discount != null)
            {
                discount.UsageCount++;
            }

            // Clear cart
            db.CartItems.RemoveRange(cartItems);

            db.SaveChanges();

            return RedirectToAction("Confirmation", new { id = order.OrderId });
        }

        // GET: Checkout/Confirmation
        public ActionResult Confirmation(int id)
        {
            var userId = User.Identity.GetUserId();
            var order = db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.ShopOrders.Select(so => so.Shop))
                .Include(o => o.ShopOrders.Select(so => so.OrderItems.Select(oi => oi.Product)))
                .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            var viewModel = new OrderConfirmationViewModel
            {
                Order = order,
                Message = "Thank you for your order! Your order has been placed successfully."
            };

            return View(viewModel);
        }

        // POST: Checkout/ApplyDiscount (AJAX)
        [HttpPost]
        public JsonResult ApplyDiscount(string discountCode, decimal subtotal)
        {
            if (string.IsNullOrEmpty(discountCode))
            {
                return Json(new { success = false, message = "Please enter a discount code." });
            }

            var discount = db.Discounts.FirstOrDefault(d =>
                d.Code.ToUpper() == discountCode.ToUpper()
                && d.IsActive
                && (!d.ExpiresAt.HasValue || d.ExpiresAt > DateTime.UtcNow)
                && (!d.UsageLimit.HasValue || d.UsageCount < d.UsageLimit)
                && d.MinimumOrderAmount <= subtotal);

            if (discount == null)
            {
                return Json(new { success = false, message = "Invalid or expired discount code." });
            }

            var discountAmount = discount.DiscountType == DiscountType.Percentage
                ? subtotal * (discount.Value / 100)
                : discount.Value;

            if (discountAmount > subtotal)
            {
                discountAmount = subtotal;
            }

            return Json(new
            {
                success = true,
                discountAmount,
                newTotal = subtotal - discountAmount,
                message = "Discount applied successfully!"
            });
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
