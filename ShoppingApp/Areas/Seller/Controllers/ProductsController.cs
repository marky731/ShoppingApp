using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;
using ShoppingApp.Models.ViewModels;

namespace ShoppingApp.Areas.Seller.Controllers
{
    [Authorize(Roles = "seller")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Seller/Products
        public ActionResult Index(string searchTerm, bool? isActive, int page = 1)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var query = db.Products
                .Include(p => p.Category)
                .Where(p => p.ShopId == shop.ShopId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.ProductName.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var viewModel = new SellerProductListViewModel
            {
                Products = query.OrderByDescending(p => p.CreatedAt).ToPagedList(page, 10),
                SearchTerm = searchTerm,
                IsActive = isActive
            };

            return View(viewModel);
        }

        // GET: Seller/Products/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                TempData["Error"] = "Your shop must be approved before adding products.";
                return RedirectToAction("Index", "Dashboard");
            }

            var viewModel = new CreateProductViewModel
            {
                Categories = db.Categories.OrderBy(c => c.CategoryName).ToList()
            };

            return View(viewModel);
        }

        // POST: Seller/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                var slug = GenerateSlug(model.ProductName);

                var product = new Product
                {
                    ShopId = shop.ShopId,
                    CategoryId = model.CategoryId,
                    ProductName = model.ProductName,
                    Slug = slug,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    MainImageUrl = model.MainImageUrl,
                    Brand = model.Brand,
                    Model = model.Model,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Products.Add(product);
                db.SaveChanges();

                // Add specifications
                if (model.Specifications != null)
                {
                    foreach (var spec in model.Specifications.Where(s => !string.IsNullOrEmpty(s.SpecName)))
                    {
                        var specification = new ProductSpecification
                        {
                            ProductId = product.ProductId,
                            SpecName = spec.SpecName,
                            SpecValue = spec.SpecValue,
                            DisplayOrder = spec.DisplayOrder
                        };
                        db.ProductSpecifications.Add(specification);
                    }
                }

                // Add additional images
                if (model.AdditionalImageUrls != null)
                {
                    foreach (var imageUrl in model.AdditionalImageUrls.Where(u => !string.IsNullOrEmpty(u)))
                    {
                        var image = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = imageUrl
                        };
                        db.ProductImages.Add(image);
                    }
                }

                db.SaveChanges();

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }

            model.Categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            return View(model);
        }

        // GET: Seller/Products/Edit/5
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var product = db.Products
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.ProductId == id && p.ShopId == shop.ShopId);

            if (product == null)
            {
                return HttpNotFound();
            }

            var viewModel = new EditProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MainImageUrl = product.MainImageUrl,
                Brand = product.Brand,
                Model = product.Model,
                IsActive = product.IsActive,
                Categories = db.Categories.OrderBy(c => c.CategoryName).ToList(),
                Specifications = product.Specifications.Select(s => new ProductSpecificationViewModel
                {
                    SpecificationId = s.SpecificationId,
                    SpecName = s.SpecName,
                    SpecValue = s.SpecValue,
                    DisplayOrder = s.DisplayOrder
                }).ToList(),
                ExistingImages = product.Images.ToList()
            };

            return View(viewModel);
        }

        // POST: Seller/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProductViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var product = db.Products
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.ProductId == model.ProductId && p.ShopId == shop.ShopId);

            if (product == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                product.ProductName = model.ProductName;
                product.Slug = GenerateSlug(model.ProductName);
                product.Description = model.Description;
                product.CategoryId = model.CategoryId;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.MainImageUrl = model.MainImageUrl;
                product.Brand = model.Brand;
                product.Model = model.Model;
                product.IsActive = model.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                // Update specifications
                db.ProductSpecifications.RemoveRange(product.Specifications);
                if (model.Specifications != null)
                {
                    foreach (var spec in model.Specifications.Where(s => !string.IsNullOrEmpty(s.SpecName)))
                    {
                        var specification = new ProductSpecification
                        {
                            ProductId = product.ProductId,
                            SpecName = spec.SpecName,
                            SpecValue = spec.SpecValue,
                            DisplayOrder = spec.DisplayOrder
                        };
                        db.ProductSpecifications.Add(specification);
                    }
                }

                // Add new images
                if (model.AdditionalImageUrls != null)
                {
                    foreach (var imageUrl in model.AdditionalImageUrls.Where(u => !string.IsNullOrEmpty(u)))
                    {
                        var image = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = imageUrl
                        };
                        db.ProductImages.Add(image);
                    }
                }

                db.SaveChanges();

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }

            model.Categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            model.ExistingImages = product.Images.ToList();
            return View(model);
        }

        // GET: Seller/Products/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .Include(p => p.Reviews.Select(r => r.User))
                .FirstOrDefault(p => p.ProductId == id && p.ShopId == shop.ShopId);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Seller/Products/Delete/5
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

            var product = db.Products.FirstOrDefault(p => p.ProductId == id && p.ShopId == shop.ShopId);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Check if product has orders
            var hasOrders = db.OrderItems.Any(oi => oi.ProductId == id);
            if (hasOrders)
            {
                // Soft delete - just deactivate
                product.IsActive = false;
                db.SaveChanges();
                TempData["Success"] = "Product has been deactivated (it has existing orders).";
            }
            else
            {
                // Hard delete
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        // POST: Seller/Products/DeleteImage/5
        [HttpPost]
        public JsonResult DeleteImage(int imageId)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return Json(new { success = false, message = "Shop not found." });
            }

            var image = db.ProductImages
                .Include(i => i.Product)
                .FirstOrDefault(i => i.ImageId == imageId && i.Product.ShopId == shop.ShopId);

            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            db.ProductImages.Remove(image);
            db.SaveChanges();

            return Json(new { success = true });
        }

        private string GenerateSlug(string name)
        {
            var slug = name.ToLower();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            slug = Regex.Replace(slug, @"-+", "-");

            // Ensure uniqueness
            var baseSlug = slug;
            var counter = 1;
            while (db.Products.Any(p => p.Slug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
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
