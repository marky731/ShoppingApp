using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class SellersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Sellers/Pending
        public ActionResult Pending(int page = 1)
        {
            var pendingShops = db.Shops
                .Include(s => s.Seller)
                .Where(s => !s.IsApproved)
                .OrderBy(s => s.CreatedAt)
                .ToPagedList(page, 10);

            var viewModel = new PendingSellersViewModel
            {
                PendingShops = pendingShops
            };

            return View(viewModel);
        }

        // POST: Admin/Sellers/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var shop = db.Shops.FirstOrDefault(s => s.ShopId == id);

            if (shop == null)
            {
                return HttpNotFound();
            }

            shop.IsApproved = true;
            shop.UpdatedAt = DateTime.UtcNow;

            // Add notification
            var notification = new Notification
            {
                UserId = shop.SellerId,
                Message = "Your shop has been approved! You can now start adding products.",
                LinkUrl = "/Seller/Dashboard",
                CreatedAt = DateTime.UtcNow
            };
            db.Notifications.Add(notification);

            db.SaveChanges();

            TempData["Success"] = "Shop approved successfully.";
            return RedirectToAction("Pending");
        }

        // POST: Admin/Sellers/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            var shop = db.Shops.FirstOrDefault(s => s.ShopId == id);

            if (shop == null)
            {
                return HttpNotFound();
            }

            // Add notification
            var notification = new Notification
            {
                UserId = shop.SellerId,
                Message = "Your shop application has been rejected. Please contact support for more information.",
                CreatedAt = DateTime.UtcNow
            };
            db.Notifications.Add(notification);

            // Delete the shop
            db.Shops.Remove(shop);
            db.SaveChanges();

            TempData["Success"] = "Shop rejected and deleted.";
            return RedirectToAction("Pending");
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
