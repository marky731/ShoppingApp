using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Seller.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Seller/Shop/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var existingShop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (existingShop != null)
            {
                return RedirectToAction("Edit");
            }

            return View(new ShopViewModel());
        }

        // POST: Seller/Shop/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ShopViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var existingShop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

                if (existingShop != null)
                {
                    TempData["Error"] = "You already have a shop.";
                    return RedirectToAction("Edit");
                }

                var shop = new Shop
                {
                    SellerId = userId,
                    ShopName = model.Name,
                    Description = model.Description,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Shops.Add(shop);
                db.SaveChanges();

                // Add seller role if not already assigned
                if (!await UserManager.IsInRoleAsync(userId, "seller"))
                {
                    await UserManager.AddToRoleAsync(userId, "seller");
                }

                TempData["Success"] = "Shop created successfully! It will be visible after admin approval.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        // GET: Seller/Shop/Edit
        [Authorize(Roles = "seller")]
        public ActionResult Edit()
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Create");
            }

            var viewModel = new EditShopViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Description = shop.Description,
                LogoImageUrl = shop.LogoImageUrl,
                BannerImageUrl = shop.BannerImageUrl
            };

            return View(viewModel);
        }

        // POST: Seller/Shop/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "seller")]
        public ActionResult Edit(EditShopViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Edit");
            }

            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

                if (shop == null)
                {
                    return HttpNotFound();
                }

                shop.ShopName = model.ShopName;
                shop.Description = model.Description;
                shop.LogoImageUrl = model.LogoImageUrl;
                shop.BannerImageUrl = model.BannerImageUrl;
                shop.UpdatedAt = DateTime.UtcNow;

                db.SaveChanges();

                TempData["Success"] = "Shop updated successfully!";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
