using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Seller.Controllers
{
    [Authorize(Roles = "seller")]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Seller/Orders
        public ActionResult Index(OrderStatus? status, int page = 1)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var query = db.ShopOrders
                .Include(so => so.Order.User)
                .Include(so => so.Order.ShippingAddress)
                .Include(so => so.OrderItems)
                .Where(so => so.ShopId == shop.ShopId);

            if (status.HasValue)
            {
                query = query.Where(so => so.ShopOrderStatus == status.Value);
            }

            var shopOrders = query.OrderByDescending(so => so.Order.OrderDate).ToList();

            var viewModel = shopOrders.Select(so => new SellerOrderListItemViewModel
            {
                Id = so.ShopOrderId,
                OrderNumber = so.Order?.OrderId.ToString() ?? "N/A",
                CreatedAt = so.Order?.OrderDate ?? DateTime.MinValue,
                CustomerName = so.Order?.User != null ? so.Order.User.FirstName + " " + so.Order.User.LastName : "Unknown",
                CustomerEmail = so.Order?.User?.Email ?? "",
                ItemCount = so.OrderItems?.Count ?? 0,
                Total = so.ShopTotal,
                Status = so.ShopOrderStatus
            });

            ViewBag.StatusFilter = status;

            return View(viewModel.ToPagedList(page, 10));
        }

        // GET: Seller/Orders/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopOrder = db.ShopOrders
                .Include(so => so.Order.User)
                .Include(so => so.Order.ShippingAddress)
                .Include(so => so.OrderItems.Select(oi => oi.Product.Images))
                .FirstOrDefault(so => so.ShopOrderId == id && so.ShopId == shop.ShopId);

            if (shopOrder == null)
            {
                return HttpNotFound();
            }

            var shippingAddr = shopOrder.Order?.ShippingAddress;
            var shippingAddressStr = shippingAddr != null
                ? $"{shippingAddr.StreetAddress}, {shippingAddr.City}, {shippingAddr.PostalCode}, {shippingAddr.Country}"
                : "N/A";

            var viewModel = new SellerOrderDetailsViewModelNew
            {
                Id = shopOrder.ShopOrderId,
                OrderNumber = shopOrder.Order?.OrderId.ToString() ?? "N/A",
                CreatedAt = shopOrder.Order?.OrderDate ?? DateTime.MinValue,
                CustomerName = shopOrder.Order?.User != null ? shopOrder.Order.User.FirstName + " " + shopOrder.Order.User.LastName : "Unknown",
                CustomerEmail = shopOrder.Order?.User?.Email ?? "",
                Status = shopOrder.ShopOrderStatus,
                Items = shopOrder.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.Product.ProductName,
                    Sku = oi.Product.ProductId.ToString(),
                    UnitPrice = oi.PriceAtPurchase,
                    Quantity = oi.Quantity,
                    Total = oi.PriceAtPurchase * oi.Quantity,
                    ProductImageUrl = oi.Product.Images.FirstOrDefault()?.ImageUrl
                }),
                Subtotal = shopOrder.ShopTotal,
                DiscountAmount = 0,
                DiscountCode = "",
                Total = shopOrder.ShopTotal,
                ShippingAddress = shippingAddressStr,
                Notes = ""
            };

            return View(viewModel);
        }

        // POST: Seller/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(UpdateOrderStatusViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopOrder = db.ShopOrders.FirstOrDefault(so => so.ShopOrderId == model.ShopOrderId && so.ShopId == shop.ShopId);

            if (shopOrder == null)
            {
                return HttpNotFound();
            }

            // Validate status transition
            var validTransition = IsValidStatusTransition(shopOrder.ShopOrderStatus, model.Status);
            if (!validTransition)
            {
                TempData["Error"] = "Invalid status transition.";
                return RedirectToAction("Details", new { id = model.ShopOrderId });
            }

            shopOrder.ShopOrderStatus = model.Status;

            if (!string.IsNullOrEmpty(model.TrackingNumber))
            {
                shopOrder.TrackingNumber = model.TrackingNumber;
            }

            // Update shop's total sales if delivered
            if (model.Status == OrderStatus.Delivered)
            {
                shop.TotalSales++;
            }

            db.SaveChanges();

            TempData["Success"] = "Order status updated successfully!";
            return RedirectToAction("Details", new { id = model.ShopOrderId });
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            switch (currentStatus)
            {
                case OrderStatus.Pending:
                    return newStatus == OrderStatus.Confirmed || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Confirmed:
                    return newStatus == OrderStatus.Processing || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Processing:
                    return newStatus == OrderStatus.Shipped;
                case OrderStatus.Shipped:
                    return newStatus == OrderStatus.Delivered;
                default:
                    return false;
            }
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
