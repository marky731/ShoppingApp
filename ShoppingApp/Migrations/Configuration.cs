using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Seed Roles
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists("admin"))
            {
                roleManager.Create(new IdentityRole("admin"));
            }
            if (!roleManager.RoleExists("seller"))
            {
                roleManager.Create(new IdentityRole("seller"));
            }
            if (!roleManager.RoleExists("customer"))
            {
                roleManager.Create(new IdentityRole("customer"));
            }

            context.SaveChanges();

            // Seed Users
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false
            };

            // Admin User
            if (userManager.FindByEmail("admin@example.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = userManager.Create(admin, "Test1234");
                if (result.Succeeded)
                {
                    userManager.AddToRole(admin.Id, "admin");
                }
            }

            // Seller User 1 - TechHub
            if (userManager.FindByEmail("seller@example.com") == null)
            {
                var seller = new ApplicationUser
                {
                    UserName = "seller@example.com",
                    Email = "seller@example.com",
                    FirstName = "John",
                    LastName = "Tech",
                    Phone = "+1234567890",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = userManager.Create(seller, "Test1234");
                if (result.Succeeded)
                {
                    userManager.AddToRole(seller.Id, "seller");
                }
            }

            // Seller User 2 - Fashion Forward
            if (userManager.FindByEmail("seller2@example.com") == null)
            {
                var seller2 = new ApplicationUser
                {
                    UserName = "seller2@example.com",
                    Email = "seller2@example.com",
                    FirstName = "Sarah",
                    LastName = "Fashion",
                    Phone = "+1234567891",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result2 = userManager.Create(seller2, "Test1234");
                if (result2.Succeeded)
                {
                    userManager.AddToRole(seller2.Id, "seller");
                }
            }

            // Seller User 3 - HomeStyle
            if (userManager.FindByEmail("seller3@example.com") == null)
            {
                var seller3 = new ApplicationUser
                {
                    UserName = "seller3@example.com",
                    Email = "seller3@example.com",
                    FirstName = "Mike",
                    LastName = "Home",
                    Phone = "+1234567892",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result3 = userManager.Create(seller3, "Test1234");
                if (result3.Succeeded)
                {
                    userManager.AddToRole(seller3.Id, "seller");
                }
            }

            // Seller User 4 - Time & Style
            if (userManager.FindByEmail("seller4@example.com") == null)
            {
                var seller4 = new ApplicationUser
                {
                    UserName = "seller4@example.com",
                    Email = "seller4@example.com",
                    FirstName = "Emma",
                    LastName = "Style",
                    Phone = "+1234567893",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result4 = userManager.Create(seller4, "Test1234");
                if (result4.Succeeded)
                {
                    userManager.AddToRole(seller4.Id, "seller");
                }
            }

            // Customer User
            if (userManager.FindByEmail("john@example.com") == null)
            {
                var customer = new ApplicationUser
                {
                    UserName = "john@example.com",
                    Email = "john@example.com",
                    FirstName = "John",
                    LastName = "Customer",
                    Phone = "+1987654321",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var resultCustomer = userManager.Create(customer, "Test1234");
                if (resultCustomer.Succeeded)
                {
                    userManager.AddToRole(customer.Id, "customer");
                }
            }

            context.SaveChanges();

            // Seed Categories
            if (!context.Categories.Any())
            {
                // Parent Categories
                var electronics = new Category { CategoryName = "Electronics" };
                var clothing = new Category { CategoryName = "Clothing" };
                var home = new Category { CategoryName = "Home & Living" };
                var beauty = new Category { CategoryName = "Beauty & Personal Care" };
                var accessories = new Category { CategoryName = "Accessories" };

                context.Categories.AddOrUpdate(c => c.CategoryName, electronics, clothing, home, beauty, accessories);
                context.SaveChanges();

                // Subcategories
                context.Categories.AddOrUpdate(c => c.CategoryName,
                    new Category { CategoryName = "Smartphones", ParentCategoryId = electronics.CategoryId },
                    new Category { CategoryName = "Laptops", ParentCategoryId = electronics.CategoryId },
                    new Category { CategoryName = "Bags", ParentCategoryId = clothing.CategoryId },
                    new Category { CategoryName = "Shoes", ParentCategoryId = clothing.CategoryId },
                    new Category { CategoryName = "Kitchen", ParentCategoryId = home.CategoryId },
                    new Category { CategoryName = "Watches", ParentCategoryId = accessories.CategoryId }
                );
                context.SaveChanges();
            }

            // Seed Shops
            var sellerUser1 = context.Users.FirstOrDefault(u => u.Email == "seller@example.com");
            var sellerUser2 = context.Users.FirstOrDefault(u => u.Email == "seller2@example.com");
            var sellerUser3 = context.Users.FirstOrDefault(u => u.Email == "seller3@example.com");
            var sellerUser4 = context.Users.FirstOrDefault(u => u.Email == "seller4@example.com");

            Shop techHub = null, fashionForward = null, homeStyle = null, timeAndStyle = null;

            if (sellerUser1 != null && !context.Shops.Any(s => s.ShopName == "TechHub"))
            {
                techHub = new Shop
                {
                    SellerId = sellerUser1.Id,
                    ShopName = "TechHub",
                    Description = "Your one-stop shop for the latest electronics and gadgets. We offer premium quality tech products with excellent customer service.",
                    IsApproved = true,
                    AverageRating = 4.7m,
                    TotalSales = 523,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Shops.Add(techHub);
            }
            else
            {
                techHub = context.Shops.FirstOrDefault(s => s.ShopName == "TechHub");
            }

            if (sellerUser2 != null && !context.Shops.Any(s => s.ShopName == "Fashion Forward"))
            {
                fashionForward = new Shop
                {
                    SellerId = sellerUser2.Id,
                    ShopName = "Fashion Forward",
                    Description = "Trendy and sustainable fashion for the modern individual. Quality clothing and accessories that make a statement.",
                    IsApproved = true,
                    AverageRating = 4.5m,
                    TotalSales = 312,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Shops.Add(fashionForward);
            }
            else
            {
                fashionForward = context.Shops.FirstOrDefault(s => s.ShopName == "Fashion Forward");
            }

            if (sellerUser3 != null && !context.Shops.Any(s => s.ShopName == "HomeStyle"))
            {
                homeStyle = new Shop
                {
                    SellerId = sellerUser3.Id,
                    ShopName = "HomeStyle",
                    Description = "Transform your living space with our curated collection of home essentials. From kitchen to bedroom, we've got you covered.",
                    IsApproved = true,
                    AverageRating = 4.6m,
                    TotalSales = 245,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Shops.Add(homeStyle);
            }
            else
            {
                homeStyle = context.Shops.FirstOrDefault(s => s.ShopName == "HomeStyle");
            }

            if (sellerUser4 != null && !context.Shops.Any(s => s.ShopName == "Time & Style"))
            {
                timeAndStyle = new Shop
                {
                    SellerId = sellerUser4.Id,
                    ShopName = "Time & Style",
                    Description = "Elegant timepieces and accessories for every occasion. From classic to contemporary, find your perfect watch.",
                    IsApproved = true,
                    AverageRating = 4.8m,
                    TotalSales = 178,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Shops.Add(timeAndStyle);
            }
            else
            {
                timeAndStyle = context.Shops.FirstOrDefault(s => s.ShopName == "Time & Style");
            }

            context.SaveChanges();

            // Get categories for products
            var smartphones = context.Categories.FirstOrDefault(c => c.CategoryName == "Smartphones");
            var laptops = context.Categories.FirstOrDefault(c => c.CategoryName == "Laptops");
            var bags = context.Categories.FirstOrDefault(c => c.CategoryName == "Bags");
            var shoes = context.Categories.FirstOrDefault(c => c.CategoryName == "Shoes");
            var kitchen = context.Categories.FirstOrDefault(c => c.CategoryName == "Kitchen");
            var watches = context.Categories.FirstOrDefault(c => c.CategoryName == "Watches");

            // Seed Products for TechHub - Smartphones
            if (techHub != null && smartphones != null && !context.Products.Any(p => p.Slug == "iphone-15-pro"))
            {
                var iphone15 = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = smartphones.CategoryId,
                    ProductName = "iPhone 15 Pro",
                    Slug = "iphone-15-pro",
                    Description = "The most powerful iPhone ever. Features a titanium design, A17 Pro chip, and an advanced camera system for stunning photos and videos.",
                    Price = 1199.99m,
                    StockQuantity = 50,
                    Brand = "Apple",
                    AverageRating = 4.8m,
                    TotalReviews = 156,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(iphone15);
                context.SaveChanges();

                iphone15.MainImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = iphone15.ProductId, ImageUrl = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=500" });
                context.ProductImages.Add(new ProductImage { ProductId = iphone15.ProductId, ImageUrl = "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?w=500" });

                context.ProductSpecifications.Add(new ProductSpecification { ProductId = iphone15.ProductId, SpecName = "Display", SpecValue = "6.1-inch Super Retina XDR", DisplayOrder = 1 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = iphone15.ProductId, SpecName = "Chip", SpecValue = "A17 Pro", DisplayOrder = 2 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = iphone15.ProductId, SpecName = "Storage", SpecValue = "256GB", DisplayOrder = 3 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = iphone15.ProductId, SpecName = "Camera", SpecValue = "48MP Main + 12MP Ultra Wide + 12MP Telephoto", DisplayOrder = 4 });
            }

            if (techHub != null && smartphones != null && !context.Products.Any(p => p.Slug == "samsung-galaxy-s24-ultra"))
            {
                var galaxyS24 = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = smartphones.CategoryId,
                    ProductName = "Samsung Galaxy S24 Ultra",
                    Slug = "samsung-galaxy-s24-ultra",
                    Description = "Experience the pinnacle of Android innovation with Galaxy AI, a stunning 200MP camera, and S Pen integration.",
                    Price = 1299.99m,
                    StockQuantity = 35,
                    Brand = "Samsung",
                    AverageRating = 4.7m,
                    TotalReviews = 89,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(galaxyS24);
                context.SaveChanges();

                galaxyS24.MainImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = galaxyS24.ProductId, ImageUrl = "https://images.unsplash.com/photo-1610945264803-c22b62d2a7b3?w=500" });
                context.ProductImages.Add(new ProductImage { ProductId = galaxyS24.ProductId, ImageUrl = "https://images.unsplash.com/photo-1585060544812-6b45742d762f?w=500" });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = galaxyS24.ProductId, SpecName = "Display", SpecValue = "6.8-inch Dynamic AMOLED 2X", DisplayOrder = 1 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = galaxyS24.ProductId, SpecName = "Processor", SpecValue = "Snapdragon 8 Gen 3", DisplayOrder = 2 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = galaxyS24.ProductId, SpecName = "RAM", SpecValue = "12GB", DisplayOrder = 3 });
            }

            if (techHub != null && smartphones != null && !context.Products.Any(p => p.Slug == "google-pixel-8-pro"))
            {
                var pixel8 = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = smartphones.CategoryId,
                    ProductName = "Google Pixel 8 Pro",
                    Slug = "google-pixel-8-pro",
                    Description = "The best of Google with advanced AI features, exceptional camera capabilities, and 7 years of updates.",
                    Price = 999.99m,
                    StockQuantity = 40,
                    Brand = "Google",
                    AverageRating = 4.6m,
                    TotalReviews = 67,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(pixel8);
                context.SaveChanges();

                pixel8.MainImageUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = pixel8.ProductId, ImageUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500" });
            }

            if (techHub != null && smartphones != null && !context.Products.Any(p => p.Slug == "oneplus-12"))
            {
                var oneplus12 = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = smartphones.CategoryId,
                    ProductName = "OnePlus 12",
                    Slug = "oneplus-12",
                    Description = "Flagship killer with Snapdragon 8 Gen 3, Hasselblad camera system, and ultra-fast 100W charging.",
                    Price = 799.99m,
                    StockQuantity = 45,
                    Brand = "OnePlus",
                    AverageRating = 4.5m,
                    TotalReviews = 43,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(oneplus12);
                context.SaveChanges();

                oneplus12.MainImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = oneplus12.ProductId, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500" });
            }

            // TechHub - Laptops
            if (techHub != null && laptops != null && !context.Products.Any(p => p.Slug == "macbook-pro-14"))
            {
                var macbook = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = laptops.CategoryId,
                    ProductName = "MacBook Pro 14\"",
                    Slug = "macbook-pro-14",
                    Description = "Supercharged by M3 Pro chip. Stunning Liquid Retina XDR display. All-day battery life. The most advanced MacBook Pro ever.",
                    Price = 1999.99m,
                    StockQuantity = 25,
                    Brand = "Apple",
                    AverageRating = 4.9m,
                    TotalReviews = 78,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(macbook);
                context.SaveChanges();

                macbook.MainImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = macbook.ProductId, ImageUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=500" });
                context.ProductImages.Add(new ProductImage { ProductId = macbook.ProductId, ImageUrl = "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?w=500" });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = macbook.ProductId, SpecName = "Chip", SpecValue = "Apple M3 Pro", DisplayOrder = 1 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = macbook.ProductId, SpecName = "RAM", SpecValue = "18GB Unified Memory", DisplayOrder = 2 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = macbook.ProductId, SpecName = "Storage", SpecValue = "512GB SSD", DisplayOrder = 3 });
                context.ProductSpecifications.Add(new ProductSpecification { ProductId = macbook.ProductId, SpecName = "Display", SpecValue = "14.2-inch Liquid Retina XDR", DisplayOrder = 4 });
            }

            if (techHub != null && laptops != null && !context.Products.Any(p => p.Slug == "dell-xps-15"))
            {
                var dellXps = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = laptops.CategoryId,
                    ProductName = "Dell XPS 15",
                    Slug = "dell-xps-15",
                    Description = "Premium Windows laptop with InfinityEdge display, 13th Gen Intel Core processors, and stunning OLED screen option.",
                    Price = 1599.99m,
                    StockQuantity = 30,
                    Brand = "Dell",
                    AverageRating = 4.6m,
                    TotalReviews = 56,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(dellXps);
                context.SaveChanges();

                dellXps.MainImageUrl = "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = dellXps.ProductId, ImageUrl = "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=500" });
                context.ProductImages.Add(new ProductImage { ProductId = dellXps.ProductId, ImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500" });
            }

            if (techHub != null && laptops != null && !context.Products.Any(p => p.Slug == "thinkpad-x1-carbon"))
            {
                var thinkpad = new Product
                {
                    ShopId = techHub.ShopId,
                    CategoryId = laptops.CategoryId,
                    ProductName = "ThinkPad X1 Carbon Gen 11",
                    Slug = "thinkpad-x1-carbon",
                    Description = "Legendary business laptop with exceptional keyboard, robust security features, and all-day battery life.",
                    Price = 1449.99m,
                    StockQuantity = 20,
                    Brand = "Lenovo",
                    AverageRating = 4.7m,
                    TotalReviews = 34,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(thinkpad);
                context.SaveChanges();

                thinkpad.MainImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = thinkpad.ProductId, ImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500" });
            }

            // Fashion Forward - Bags
            if (fashionForward != null && bags != null && !context.Products.Any(p => p.Slug == "white-linen-tote-bag"))
            {
                var toteBag = new Product
                {
                    ShopId = fashionForward.ShopId,
                    CategoryId = bags.CategoryId,
                    ProductName = "White Linen Tote Bag",
                    Slug = "white-linen-tote-bag",
                    Description = "Elegant and spacious linen tote bag perfect for everyday use. Features reinforced handles and interior pocket.",
                    Price = 79.99m,
                    StockQuantity = 60,
                    Brand = "Fashion Forward",
                    AverageRating = 4.4m,
                    TotalReviews = 28,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(toteBag);
                context.SaveChanges();

                toteBag.MainImageUrl = "https://images.unsplash.com/photo-1544816155-12df9643f363?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = toteBag.ProductId, ImageUrl = "https://images.unsplash.com/photo-1594223274512-ad4803739b7c?w=500" });
                context.ProductImages.Add(new ProductImage { ProductId = toteBag.ProductId, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=500" });
            }

            if (fashionForward != null && bags != null && !context.Products.Any(p => p.Slug == "canvas-messenger-bag"))
            {
                var messengerBag = new Product
                {
                    ShopId = fashionForward.ShopId,
                    CategoryId = bags.CategoryId,
                    ProductName = "Canvas Messenger Bag",
                    Slug = "canvas-messenger-bag",
                    Description = "Durable canvas messenger bag with adjustable strap. Perfect for work or casual outings.",
                    Price = 89.99m,
                    StockQuantity = 45,
                    Brand = "Fashion Forward",
                    AverageRating = 4.3m,
                    TotalReviews = 19,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(messengerBag);
                context.SaveChanges();

                messengerBag.MainImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = messengerBag.ProductId, ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500" });
            }

            if (fashionForward != null && bags != null && !context.Products.Any(p => p.Slug == "leather-backpack"))
            {
                var backpack = new Product
                {
                    ShopId = fashionForward.ShopId,
                    CategoryId = bags.CategoryId,
                    ProductName = "Premium Leather Backpack",
                    Slug = "leather-backpack",
                    Description = "Handcrafted leather backpack with laptop compartment. Combines style with functionality.",
                    Price = 159.99m,
                    StockQuantity = 30,
                    Brand = "Fashion Forward",
                    AverageRating = 4.6m,
                    TotalReviews = 42,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(backpack);
                context.SaveChanges();

                backpack.MainImageUrl = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = backpack.ProductId, ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500" });
                context.ProductImages.Add(new ProductImage { ProductId = backpack.ProductId, ImageUrl = "https://images.unsplash.com/photo-1622560480654-d96214fdc887?w=500" });
            }

            // Fashion Forward - Shoes
            if (fashionForward != null && shoes != null && !context.Products.Any(p => p.Slug == "white-lace-up-sneakers"))
            {
                var sneakers = new Product
                {
                    ShopId = fashionForward.ShopId,
                    CategoryId = shoes.CategoryId,
                    ProductName = "White Lace-Up Sneakers",
                    Slug = "white-lace-up-sneakers",
                    Description = "Classic white leather sneakers with cushioned insole. Versatile and comfortable for all-day wear.",
                    Price = 119.99m,
                    StockQuantity = 80,
                    Brand = "Fashion Forward",
                    AverageRating = 4.5m,
                    TotalReviews = 67,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(sneakers);
                context.SaveChanges();

                sneakers.MainImageUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = sneakers.ProductId, ImageUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=500" });
            }

            if (fashionForward != null && shoes != null && !context.Products.Any(p => p.Slug == "running-shoes"))
            {
                var runningShoes = new Product
                {
                    ShopId = fashionForward.ShopId,
                    CategoryId = shoes.CategoryId,
                    ProductName = "Performance Running Shoes",
                    Slug = "running-shoes",
                    Description = "Lightweight running shoes with responsive cushioning and breathable mesh upper.",
                    Price = 139.99m,
                    StockQuantity = 55,
                    Brand = "Fashion Forward",
                    AverageRating = 4.7m,
                    TotalReviews = 89,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(runningShoes);
                context.SaveChanges();

                runningShoes.MainImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = runningShoes.ProductId, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500" });
            }

            if (fashionForward != null && shoes != null && !context.Products.Any(p => p.Slug == "leather-loafers"))
            {
                var loafers = new Product
                {
                    ShopId = fashionForward.ShopId,
                    CategoryId = shoes.CategoryId,
                    ProductName = "Classic Leather Loafers",
                    Slug = "leather-loafers",
                    Description = "Elegant leather loafers perfect for business or smart casual occasions.",
                    Price = 149.99m,
                    StockQuantity = 40,
                    Brand = "Fashion Forward",
                    AverageRating = 4.4m,
                    TotalReviews = 31,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(loafers);
                context.SaveChanges();

                loafers.MainImageUrl = "https://images.unsplash.com/photo-1614252369475-531eba835eb1?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = loafers.ProductId, ImageUrl = "https://images.unsplash.com/photo-1614252369475-531eba835eb1?w=500" });
            }

            // HomeStyle - Kitchen
            if (homeStyle != null && kitchen != null && !context.Products.Any(p => p.Slug == "glass-water-bottle"))
            {
                var waterBottle = new Product
                {
                    ShopId = homeStyle.ShopId,
                    CategoryId = kitchen.CategoryId,
                    ProductName = "Borosilicate Glass Water Bottle",
                    Slug = "glass-water-bottle",
                    Description = "Eco-friendly glass water bottle with bamboo lid. BPA-free and perfect for hot or cold drinks.",
                    Price = 24.99m,
                    StockQuantity = 100,
                    Brand = "HomeStyle",
                    AverageRating = 4.6m,
                    TotalReviews = 78,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(waterBottle);
                context.SaveChanges();

                waterBottle.MainImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = waterBottle.ProductId, ImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=500" });
            }

            if (homeStyle != null && kitchen != null && !context.Products.Any(p => p.Slug == "ceramic-coffee-mug-set"))
            {
                var mugSet = new Product
                {
                    ShopId = homeStyle.ShopId,
                    CategoryId = kitchen.CategoryId,
                    ProductName = "Ceramic Coffee Mug Set",
                    Slug = "ceramic-coffee-mug-set",
                    Description = "Set of 4 handcrafted ceramic mugs. Microwave and dishwasher safe.",
                    Price = 39.99m,
                    StockQuantity = 70,
                    Brand = "HomeStyle",
                    AverageRating = 4.5m,
                    TotalReviews = 45,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(mugSet);
                context.SaveChanges();

                mugSet.MainImageUrl = "https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = mugSet.ProductId, ImageUrl = "https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?w=500" });
            }

            if (homeStyle != null && kitchen != null && !context.Products.Any(p => p.Slug == "bamboo-cutting-board-set"))
            {
                var cuttingBoard = new Product
                {
                    ShopId = homeStyle.ShopId,
                    CategoryId = kitchen.CategoryId,
                    ProductName = "Bamboo Cutting Board Set",
                    Slug = "bamboo-cutting-board-set",
                    Description = "Set of 3 organic bamboo cutting boards in different sizes. Naturally antimicrobial.",
                    Price = 49.99m,
                    StockQuantity = 50,
                    Brand = "HomeStyle",
                    AverageRating = 4.7m,
                    TotalReviews = 62,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(cuttingBoard);
                context.SaveChanges();

                cuttingBoard.MainImageUrl = "https://images.unsplash.com/photo-1594226801341-41427b4e5c22?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = cuttingBoard.ProductId, ImageUrl = "https://images.unsplash.com/photo-1594226801341-41427b4e5c22?w=500" });
            }

            if (homeStyle != null && kitchen != null && !context.Products.Any(p => p.Slug == "stainless-steel-kettle"))
            {
                var kettle = new Product
                {
                    ShopId = homeStyle.ShopId,
                    CategoryId = kitchen.CategoryId,
                    ProductName = "Stainless Steel Electric Kettle",
                    Slug = "stainless-steel-kettle",
                    Description = "1.7L electric kettle with temperature control. Auto shut-off and boil-dry protection.",
                    Price = 59.99m,
                    StockQuantity = 40,
                    Brand = "HomeStyle",
                    AverageRating = 4.8m,
                    TotalReviews = 93,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(kettle);
                context.SaveChanges();

                kettle.MainImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = kettle.ProductId, ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=500" });
            }

            // Time & Style - Watches
            if (timeAndStyle != null && watches != null && !context.Products.Any(p => p.Slug == "silver-minimalist-watch"))
            {
                var silverWatch = new Product
                {
                    ShopId = timeAndStyle.ShopId,
                    CategoryId = watches.CategoryId,
                    ProductName = "Silver Minimalist Watch",
                    Slug = "silver-minimalist-watch",
                    Description = "Elegant silver watch with clean minimalist design. Japanese quartz movement with genuine leather strap.",
                    Price = 189.99m,
                    StockQuantity = 35,
                    Brand = "Time & Style",
                    AverageRating = 4.7m,
                    TotalReviews = 48,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(silverWatch);
                context.SaveChanges();

                silverWatch.MainImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = silverWatch.ProductId, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500" });
            }

            if (timeAndStyle != null && watches != null && !context.Products.Any(p => p.Slug == "black-chronograph-watch"))
            {
                var chronograph = new Product
                {
                    ShopId = timeAndStyle.ShopId,
                    CategoryId = watches.CategoryId,
                    ProductName = "Black Chronograph Watch",
                    Slug = "black-chronograph-watch",
                    Description = "Sophisticated chronograph watch with matte black finish. Water resistant to 50m.",
                    Price = 249.99m,
                    StockQuantity = 25,
                    Brand = "Time & Style",
                    AverageRating = 4.8m,
                    TotalReviews = 36,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(chronograph);
                context.SaveChanges();

                chronograph.MainImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = chronograph.ProductId, ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=500" });
            }

            if (timeAndStyle != null && watches != null && !context.Products.Any(p => p.Slug == "rose-gold-watch"))
            {
                var roseGoldWatch = new Product
                {
                    ShopId = timeAndStyle.ShopId,
                    CategoryId = watches.CategoryId,
                    ProductName = "Rose Gold Elegant Watch",
                    Slug = "rose-gold-watch",
                    Description = "Beautiful rose gold watch with mother of pearl dial. Perfect for special occasions.",
                    Price = 219.99m,
                    StockQuantity = 30,
                    Brand = "Time & Style",
                    AverageRating = 4.9m,
                    TotalReviews = 52,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Products.Add(roseGoldWatch);
                context.SaveChanges();

                roseGoldWatch.MainImageUrl = "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?w=500";
                context.ProductImages.Add(new ProductImage { ProductId = roseGoldWatch.ProductId, ImageUrl = "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?w=500" });
            }

            context.SaveChanges();

            // Seed Discounts for each shop
            if (techHub != null && !context.Discounts.Any(d => d.Code == "TECH10"))
            {
                context.Discounts.Add(new Discount
                {
                    ShopId = techHub.ShopId,
                    Code = "TECH10",
                    DiscountType = DiscountType.Percentage,
                    Value = 10,
                    MinimumOrderAmount = 100,
                    UsageLimit = 100,
                    PerUserLimit = 1,
                    ExpiresAt = DateTime.UtcNow.AddMonths(3),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (fashionForward != null && !context.Discounts.Any(d => d.Code == "FASHION15"))
            {
                context.Discounts.Add(new Discount
                {
                    ShopId = fashionForward.ShopId,
                    Code = "FASHION15",
                    DiscountType = DiscountType.Percentage,
                    Value = 15,
                    MinimumOrderAmount = 50,
                    UsageLimit = 200,
                    PerUserLimit = 2,
                    ExpiresAt = DateTime.UtcNow.AddMonths(2),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (homeStyle != null && !context.Discounts.Any(d => d.Code == "HOME20"))
            {
                context.Discounts.Add(new Discount
                {
                    ShopId = homeStyle.ShopId,
                    Code = "HOME20",
                    DiscountType = DiscountType.FixedAmount,
                    Value = 20,
                    MinimumOrderAmount = 75,
                    UsageLimit = 50,
                    PerUserLimit = 1,
                    ExpiresAt = DateTime.UtcNow.AddMonths(1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (timeAndStyle != null && !context.Discounts.Any(d => d.Code == "STYLE25"))
            {
                context.Discounts.Add(new Discount
                {
                    ShopId = timeAndStyle.ShopId,
                    Code = "STYLE25",
                    DiscountType = DiscountType.Percentage,
                    Value = 25,
                    MinimumOrderAmount = 150,
                    UsageLimit = 30,
                    PerUserLimit = 1,
                    ExpiresAt = DateTime.UtcNow.AddMonths(6),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.SaveChanges();

            // Seed Address for Customer
            var customerUser = context.Users.FirstOrDefault(u => u.Email == "john@example.com");
            if (customerUser != null && !context.Addresses.Any(a => a.UserId == customerUser.Id))
            {
                context.Addresses.Add(new Address
                {
                    UserId = customerUser.Id,
                    AddressLabel = "Home",
                    StreetAddress = "123 Main Street",
                    City = "New York",
                    PostalCode = "10001",
                    Country = "United States"
                });
                context.Addresses.Add(new Address
                {
                    UserId = customerUser.Id,
                    AddressLabel = "Work",
                    StreetAddress = "456 Business Ave, Suite 100",
                    City = "New York",
                    PostalCode = "10016",
                    Country = "United States"
                });
                context.SaveChanges();
            }
        }
    }
}
