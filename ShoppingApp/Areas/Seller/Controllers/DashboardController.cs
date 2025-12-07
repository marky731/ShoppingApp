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

            var viewModel = new SellerDashboardViewModel
            {
                Shop = shop,
                TotalProducts = db.Products.Count(p => p.ShopId == shop.ShopId),
                TotalOrders = db.ShopOrders.Count(so => so.ShopId == shop.ShopId),
                TotalRevenue = db.ShopOrders
                    .Where(so => so.ShopId == shop.ShopId && so.ShopOrderStatus == OrderStatus.Delivered)
                    .Sum(so => (decimal?)so.ShopTotal) ?? 0,
                PendingOrders = db.ShopOrders.Count(so => so.ShopId == shop.ShopId && so.ShopOrderStatus == OrderStatus.Pending),
                RecentOrders = db.ShopOrders
                    .Include(so => so.Order.User)
                    .Include(so => so.OrderItems)
                    .Where(so => so.ShopId == shop.ShopId)
                    .OrderByDescending(so => so.Order.OrderDate)
                    .Take(5)
                    .ToList()
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
