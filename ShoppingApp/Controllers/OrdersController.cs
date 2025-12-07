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
    [Authorize]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Orders
        public ActionResult Index(OrderStatus? status, int page = 1)
        {
            var userId = User.Identity.GetUserId();

            var query = db.Orders
                .Include(o => o.ShopOrders)
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(o => o.ShopOrders.Any(so => so.ShopOrderStatus == status.Value));
            }

            var viewModel = new OrderListViewModel
            {
                Orders = query.OrderByDescending(o => o.OrderDate).ToPagedList(page, 10),
                StatusFilter = status
            };

            return View(viewModel);
        }

        // GET: Orders/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();

            var order = db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Discount)
                .Include(o => o.ShopOrders.Select(so => so.Shop))
                .Include(o => o.ShopOrders.Select(so => so.OrderItems.Select(oi => oi.Product)))
                .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Can cancel if all shop orders are pending or confirmed
            var canCancel = order.ShopOrders.All(so =>
                so.ShopOrderStatus == OrderStatus.Pending ||
                so.ShopOrderStatus == OrderStatus.Confirmed);

            var viewModel = new OrderDetailsViewModel
            {
                Order = order,
                ShopOrders = order.ShopOrders.ToList(),
                CanCancel = canCancel
            };

            return View(viewModel);
        }

        // POST: Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var userId = User.Identity.GetUserId();

            var order = db.Orders
                .Include(o => o.ShopOrders.Select(so => so.OrderItems.Select(oi => oi.Product)))
                .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Check if order can be cancelled
            var canCancel = order.ShopOrders.All(so =>
                so.ShopOrderStatus == OrderStatus.Pending ||
                so.ShopOrderStatus == OrderStatus.Confirmed);

            if (!canCancel)
            {
                TempData["Error"] = "This order cannot be cancelled.";
                return RedirectToAction("Details", new { id });
            }

            // Cancel all shop orders and restore stock
            foreach (var shopOrder in order.ShopOrders)
            {
                shopOrder.ShopOrderStatus = OrderStatus.Cancelled;

                // Restore stock
                foreach (var orderItem in shopOrder.OrderItems)
                {
                    orderItem.Product.StockQuantity += orderItem.Quantity;
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Order cancelled successfully. Stock has been restored.";
            return RedirectToAction("Details", new { id });
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
