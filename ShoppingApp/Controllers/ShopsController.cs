using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Controllers
{
    public class ShopsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Shops/Details/5
        public ActionResult Details(int id, int page = 1)
        {
            var shop = db.Shops
                .Include(s => s.Seller)
                .FirstOrDefault(s => s.ShopId == id && s.IsApproved);

            if (shop == null)
            {
                return HttpNotFound();
            }

            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.ShopId == id && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToPagedList(page, 12);

            ViewBag.Products = products;

            return View(shop);
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
