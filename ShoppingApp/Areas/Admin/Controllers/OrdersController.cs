using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Orders
        public ActionResult Index(string search, OrderStatus? status, int? shopId, int page = 1)
        {
            var query = db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.ShopOrders.Select(so => so.Shop))
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                int orderId;
                if (int.TryParse(search, out orderId))
                {
                    query = query.Where(o => o.OrderId == orderId);
                }
                else
                {
                    query = query.Where(o => o.User.Email.Contains(search) ||
                        o.User.FirstName.Contains(search) ||
                        o.User.LastName.Contains(search));
                }
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.ShopOrders.Any(so => so.ShopOrderStatus == status.Value));
            }

            if (shopId.HasValue)
            {
                query = query.Where(o => o.ShopOrders.Any(so => so.ShopId == shopId.Value));
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToPagedList(page, 20);

            // Calculate total revenue
            var totalRevenue = db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0;
            var totalOrders = db.Orders.Count();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.ShopId = shopId;
            ViewBag.Shops = new SelectList(db.Shops.OrderBy(s => s.ShopName).ToList(), "ShopId", "ShopName", shopId);

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int id)
        {
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.ShopOrders.Select(so => so.Shop))
                .Include(o => o.ShopOrders.Select(so => so.OrderItems.Select(i => i.Product)))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
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
