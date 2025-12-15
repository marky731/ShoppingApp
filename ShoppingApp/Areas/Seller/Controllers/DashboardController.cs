using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Seller.Controllers
{
    [Authorize(Roles = "seller")]
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Seller/Dashboard
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Create", "Shop");
            }

            if (!shop.IsApproved)
            {
                ViewBag.Message = "Your shop is pending approval. You will be notified once it's approved.";
                return View("PendingApproval", shop);
            }

            var shopId = shop.ShopId;

            var recentShopOrders = db.ShopOrders
                .Include(so => so.Order.User)
                .Include(so => so.OrderItems)
                .Where(so => so.ShopId == shopId)
                .OrderByDescending(so => so.Order.OrderDate)
                .Take(5)
                .ToList();

            var lowStockProducts = db.Products
                .Where(p => p.ShopId == shopId && p.IsActive && p.StockQuantity <= 10)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToList();

            var viewModel = new SellerDashboardViewModel
            {
                Shop = shop,
                TotalProducts = db.Products.Count(p => p.ShopId == shopId),
                TotalOrders = db.ShopOrders.Count(so => so.ShopId == shopId),
                TotalRevenue = db.ShopOrders
                    .Where(so => so.ShopId == shopId && so.ShopOrderStatus == OrderStatus.Delivered)
                    .Sum(so => (decimal?)so.ShopTotal) ?? 0,
                PendingOrders = db.ShopOrders.Count(so => so.ShopId == shopId && so.ShopOrderStatus == OrderStatus.Pending),
                RecentOrders = recentShopOrders.Select(so => new DashboardRecentOrderViewModel
                {
                    Id = so.ShopOrderId,
                    OrderNumber = so.Order?.OrderId.ToString() ?? "N/A",
                    CustomerName = so.Order?.User != null ? so.Order.User.FirstName + " " + so.Order.User.LastName : "Unknown",
                    ItemCount = so.OrderItems?.Count ?? 0,
                    Total = so.ShopTotal,
                    Status = so.ShopOrderStatus,
                    CreatedAt = so.Order?.OrderDate ?? System.DateTime.MinValue
                }),
                LowStockProducts = lowStockProducts.Select(p => new DashboardLowStockViewModel
                {
                    Id = p.ProductId,
                    Name = p.ProductName,
                    Stock = p.StockQuantity
                })
            };

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
