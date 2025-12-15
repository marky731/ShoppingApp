using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Addresses
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var addresses = db.Addresses.Where(a => a.UserId == userId && a.IsActive).ToList();
            return View(addresses);
        }

        // GET: Addresses/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AddressViewModel());
        }

        // POST: Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AddressViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var address = new Address
                {
                    UserId = userId,
                    AddressLabel = model.AddressLabel,
                    StreetAddress = model.StreetAddress,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Country = model.Country
                };

                db.Addresses.Add(address);
                db.SaveChanges();

                TempData["Success"] = "Address added successfully.";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: Addresses/Edit/5
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var address = db.Addresses.FirstOrDefault(a => a.AddressId == id && a.UserId == userId);

            if (address == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AddressViewModel
            {
                AddressId = address.AddressId,
                AddressLabel = address.AddressLabel,
                StreetAddress = address.StreetAddress,
                City = address.City,
                PostalCode = address.PostalCode,
                Country = address.Country
            };

            return View(viewModel);
        }

        // POST: Addresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var address = db.Addresses.FirstOrDefault(a => a.AddressId == model.AddressId && a.UserId == userId);

                if (address == null)
                {
                    return HttpNotFound();
                }

                address.AddressLabel = model.AddressLabel;
                address.StreetAddress = model.StreetAddress;
                address.City = model.City;
                address.PostalCode = model.PostalCode;
                address.Country = model.Country;

                db.SaveChanges();

                TempData["Success"] = "Address updated successfully.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Addresses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var address = db.Addresses.FirstOrDefault(a => a.AddressId == id && a.UserId == userId && a.IsActive);

            if (address == null)
            {
                return HttpNotFound();
            }

            // Soft delete - hide from user but preserve for order history
            address.IsActive = false;
            db.SaveChanges();

            TempData["Success"] = "Address deleted successfully.";
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
