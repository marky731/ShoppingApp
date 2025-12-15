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
        public ActionResult Index(string search, int? categoryId, string status, int page = 1)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null || !shop.IsApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var shopId = shop.ShopId;
            var query = db.Products
                .Include(p => p.Category)
                .Where(p => p.ShopId == shopId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(p => p.IsActive);
                else if (status == "inactive")
                    query = query.Where(p => !p.IsActive);
                else if (status == "lowstock")
                    query = query.Where(p => p.StockQuantity <= 10);
            }

            var products = query.OrderByDescending(p => p.CreatedAt).ToList();

            var viewModel = products.Select(p => new SellerProductListItemViewModel
            {
                Id = p.ProductId,
                Name = p.ProductName,
                Sku = p.Model ?? "",
                ImageUrl = p.MainImageUrl,
                CategoryName = p.Category?.CategoryName ?? "Uncategorized",
                Price = p.Price,
                DiscountedPrice = null,
                Stock = p.StockQuantity,
                TotalSales = 0, // Would need to calculate from order items
                IsActive = p.IsActive
            });

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Status = status;
            ViewBag.Categories = new SelectList(db.Categories.OrderBy(c => c.CategoryName).ToList(), "CategoryId", "CategoryName", categoryId);

            return View(viewModel.ToPagedList(page, 10));
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
        // Note: Parameter renamed from 'model' to 'viewModel' to avoid conflict with form field 'Model'
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel viewModel)
        {
            if (viewModel == null)
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
                var slug = GenerateSlug(viewModel.ProductName);

                var product = new Product
                {
                    ShopId = shop.ShopId,
                    CategoryId = viewModel.CategoryId,
                    ProductName = viewModel.ProductName,
                    Slug = slug,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    StockQuantity = viewModel.StockQuantity,
                    MainImageUrl = viewModel.MainImageUrl,
                    Brand = viewModel.Brand,
                    Model = viewModel.Model,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Products.Add(product);
                db.SaveChanges();

                // Add specifications
                if (viewModel.Specifications != null)
                {
                    foreach (var spec in viewModel.Specifications.Where(s => !string.IsNullOrEmpty(s.SpecName)))
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
                if (viewModel.AdditionalImageUrls != null)
                {
                    foreach (var imageUrl in viewModel.AdditionalImageUrls.Where(u => !string.IsNullOrEmpty(u)))
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

            viewModel.Categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            return View(viewModel);
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

            var shopId = shop.ShopId;
            var product = db.Products
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.ProductId == id && p.ShopId == shopId);

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
        // Note: Parameter renamed from 'model' to 'viewModel' to avoid conflict with form field 'Model'
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EditProductViewModel viewModel)
        {
            var userId = User.Identity.GetUserId();
            var shop = db.Shops.FirstOrDefault(s => s.SellerId == userId);

            if (shop == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Extract to local variable to avoid closure issues in EF6
            var shopId = shop.ShopId;
            var productId = viewModel?.ProductId > 0 ? viewModel.ProductId : id;

            var product = db.Products
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.ProductId == productId && p.ShopId == shopId);

            if (product == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                product.ProductName = viewModel.ProductName;
                product.Slug = GenerateSlug(viewModel.ProductName);
                product.Description = viewModel.Description;
                product.CategoryId = viewModel.CategoryId;
                product.Price = viewModel.Price;
                product.StockQuantity = viewModel.StockQuantity;
                product.MainImageUrl = viewModel.MainImageUrl;
                product.Brand = viewModel.Brand;
                product.Model = viewModel.Model;
                product.IsActive = viewModel.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                // Update specifications
                db.ProductSpecifications.RemoveRange(product.Specifications);
                if (viewModel.Specifications != null)
                {
                    foreach (var spec in viewModel.Specifications.Where(s => !string.IsNullOrEmpty(s.SpecName)))
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
                if (viewModel.AdditionalImageUrls != null)
                {
                    foreach (var imageUrl in viewModel.AdditionalImageUrls.Where(u => !string.IsNullOrEmpty(u)))
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

            viewModel.Categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            viewModel.ExistingImages = product.Images.ToList();
            return View(viewModel);
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

            var shopId = shop.ShopId;
            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .Include(p => p.Reviews.Select(r => r.User))
                .FirstOrDefault(p => p.ProductId == id && p.ShopId == shopId);

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

            var shopId = shop.ShopId;
            var product = db.Products.FirstOrDefault(p => p.ProductId == id && p.ShopId == shopId);

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

            var shopId = shop.ShopId;
            var image = db.ProductImages
                .Include(i => i.Product)
                .FirstOrDefault(i => i.ImageId == imageId && i.Product.ShopId == shopId);

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
            if (string.IsNullOrEmpty(name))
                return "product-" + Guid.NewGuid().ToString("N").Substring(0, 8);

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
