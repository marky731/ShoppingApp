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
    public class DiscountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Seller/Discounts
        public ActionResult Index(bool? isActive, int page = 1)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var query = db.Discounts.Where(d => d.ShopId == shopId);

            if (isActive.HasValue)
            {
                query = query.Where(d => d.IsActive == isActive.Value);
            }

            var discounts = query.OrderByDescending(d => d.CreatedAt).ToList();

            var viewModel = discounts.Select(d => new DiscountListItemViewModel
            {
                Id = d.DiscountId,
                Code = d.Code,
                Type = d.DiscountType,
                Value = d.Value,
                UsageCount = d.UsageCount,
                UsageLimit = d.UsageLimit,
                StartDate = null,
                EndDate = d.ExpiresAt,
                MinimumPurchase = d.MinimumOrderAmount,
                IsActive = d.IsActive
            });

            ViewBag.IsActive = isActive;

            return View(viewModel);
        }

        // GET: Seller/Discounts/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new DiscountViewModel());
        }

        // POST: Seller/Discounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DiscountViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Create");
            }

            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                var code = model.Code?.ToUpper() ?? "";

                // Check if code already exists
                if (db.Discounts.Any(d => d.Code == code))
                {
                    ModelState.AddModelError("Code", "This discount code already exists.");
                    return View(model);
                }

                var discount = new Discount
                {
                    ShopId = shop.ShopId,
                    Code = code,
                    DiscountType = model.Type,
                    Value = model.Value,
                    MinimumOrderAmount = model.MinimumPurchase ?? 0,
                    UsageLimit = model.UsageLimit,
                    PerUserLimit = 1,
                    ExpiresAt = model.EndDate,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                db.Discounts.Add(discount);
                db.SaveChanges();

                TempData["Success"] = "Discount created successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Seller/Discounts/Edit/5
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var discount = db.Discounts.FirstOrDefault(d => d.DiscountId == id && d.ShopId == shopId);

            if (discount == null)
            {
                return HttpNotFound();
            }

            var viewModel = new DiscountViewModel
            {
                Id = discount.DiscountId,
                Code = discount.Code,
                Type = discount.DiscountType,
                Value = discount.Value,
                MinimumPurchase = discount.MinimumOrderAmount,
                UsageLimit = discount.UsageLimit,
                StartDate = null,
                EndDate = discount.ExpiresAt,
                IsActive = discount.IsActive,
                UsageCount = discount.UsageCount
            };

            return View(viewModel);
        }

        // POST: Seller/Discounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DiscountViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var discountId = model.Id;
            var discount = db.Discounts.FirstOrDefault(d => d.DiscountId == discountId && d.ShopId == shopId);

            if (discount == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                var code = model.Code?.ToUpper() ?? "";

                // Check if code already exists (excluding current discount)
                if (db.Discounts.Any(d => d.Code == code && d.DiscountId != model.Id))
                {
                    ModelState.AddModelError("Code", "This discount code already exists.");
                    model.UsageCount = discount.UsageCount;
                    return View(model);
                }

                discount.Code = code;
                discount.DiscountType = model.Type;
                discount.Value = model.Value;
                discount.MinimumOrderAmount = model.MinimumPurchase ?? 0;
                discount.UsageLimit = model.UsageLimit;
                discount.ExpiresAt = model.EndDate;
                discount.IsActive = model.IsActive;

                db.SaveChanges();

                TempData["Success"] = "Discount updated successfully!";
                return RedirectToAction("Index");
            }

            model.UsageCount = discount.UsageCount;
            return View(model);
        }

        // POST: Seller/Discounts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var discount = db.Discounts.FirstOrDefault(d => d.DiscountId == id && d.ShopId == shopId);

            if (discount == null)
            {
                return HttpNotFound();
            }

            // Check if discount is used in any orders
            var isUsed = db.Orders.Any(o => o.DiscountId == id) || db.ShopOrders.Any(so => so.DiscountId == id);
            if (isUsed)
            {
                // Soft delete - just deactivate
                discount.IsActive = false;
                db.SaveChanges();
                TempData["Success"] = "Discount has been deactivated (it has been used in orders).";
            }
            else
            {
                db.Discounts.Remove(discount);
                db.SaveChanges();
                TempData["Success"] = "Discount deleted successfully!";
            }

            return RedirectToAction("Index");
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
