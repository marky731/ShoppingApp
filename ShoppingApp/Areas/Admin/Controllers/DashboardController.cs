using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = db.Users.Count(),
                TotalSellers = db.Shops.Count(s => s.IsApproved),
                TotalShops = db.Shops.Count(s => s.IsApproved),
                TotalProducts = db.Products.Count(p => p.IsActive),
                TotalOrders = db.Orders.Count(),
                TotalCategories = db.Categories.Count(),
                TotalRevenue = db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0,
                PendingSellers = db.Shops.Count(s => !s.IsApproved),
                PendingReviews = db.Reviews.Count(r => r.Status == ReviewStatus.Pending),
                RecentOrders = db.Orders
                    .Include(o => o.User)
                    .Include(o => o.ShopOrders)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToList(),
                RecentUsers = db.Users
                    .OrderByDescending(u => u.CreatedAt)
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
