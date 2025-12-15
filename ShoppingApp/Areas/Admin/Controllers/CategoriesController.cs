using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Categories
        public ActionResult Index()
        {
            var categories = db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.CategoryName)
                .ToList();

            var viewModel = categories.Select(c => new CategoryListItemViewModel
            {
                Id = c.CategoryId,
                Name = c.CategoryName,
                Description = null, // Category model doesn't have Description
                ParentId = c.ParentCategoryId,
                ParentName = c.ParentCategory?.CategoryName,
                ProductCount = c.Products.Count,
                SubCategoryCount = c.SubCategories.Count,
                DisplayOrder = 0, // Category model doesn't have DisplayOrder
                IsActive = true // Category model doesn't have IsActive
            });

            return View(viewModel);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            var viewModel = new CreateCategoryViewModel
            {
                ParentCategories = db.Categories.Where(c => c.ParentCategoryId == null).ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if category name already exists
                if (db.Categories.Any(c => c.CategoryName == model.CategoryName))
                {
                    ModelState.AddModelError("CategoryName", "A category with this name already exists.");
                    model.ParentCategories = db.Categories.Where(c => c.ParentCategoryId == null).ToList();
                    return View(model);
                }

                var category = new Category
                {
                    CategoryName = model.CategoryName,
                    ParentCategoryId = model.ParentCategoryId
                };

                db.Categories.Add(category);
                db.SaveChanges();

                TempData["Success"] = "Category created successfully!";
                return RedirectToAction("Index");
            }

            model.ParentCategories = db.Categories.Where(c => c.ParentCategoryId == null).ToList();
            return View(model);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int id)
        {
            var category = db.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            var viewModel = new EditCategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategories = db.Categories
                    .Where(c => c.ParentCategoryId == null && c.CategoryId != id)
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditCategoryViewModel model)
        {
            var category = db.Categories.FirstOrDefault(c => c.CategoryId == model.CategoryId);

            if (category == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if category name already exists (excluding current)
                if (db.Categories.Any(c => c.CategoryName == model.CategoryName && c.CategoryId != model.CategoryId))
                {
                    ModelState.AddModelError("CategoryName", "A category with this name already exists.");
                    model.ParentCategories = db.Categories
                        .Where(c => c.ParentCategoryId == null && c.CategoryId != model.CategoryId)
                        .ToList();
                    return View(model);
                }

                // Prevent circular reference
                if (model.ParentCategoryId == model.CategoryId)
                {
                    ModelState.AddModelError("ParentCategoryId", "A category cannot be its own parent.");
                    model.ParentCategories = db.Categories
                        .Where(c => c.ParentCategoryId == null && c.CategoryId != model.CategoryId)
                        .ToList();
                    return View(model);
                }

                category.CategoryName = model.CategoryName;
                category.ParentCategoryId = model.ParentCategoryId;

                db.SaveChanges();

                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }

            model.ParentCategories = db.Categories
                .Where(c => c.ParentCategoryId == null && c.CategoryId != model.CategoryId)
                .ToList();
            return View(model);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Check if category has products
            if (category.Products.Any())
            {
                TempData["Error"] = "Cannot delete category with existing products.";
                return RedirectToAction("Index");
            }

            // Check if category has subcategories
            if (category.SubCategories.Any())
            {
                TempData["Error"] = "Cannot delete category with subcategories.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Category deleted successfully!";
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
