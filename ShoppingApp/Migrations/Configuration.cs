using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ShoppingApp.Models.Domain;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
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

            // Seed Users
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.PasswordValidator = new PasswordValidator { RequiredLength = 6 };

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
                userManager.Create(admin, "Test1234");
                userManager.AddToRole(admin.Id, "admin");
            }

            // Seller User
            if (userManager.FindByEmail("seller@example.com") == null)
            {
                var seller = new ApplicationUser
                {
                    UserName = "seller@example.com",
                    Email = "seller@example.com",
                    FirstName = "John",
                    LastName = "Seller",
                    Phone = "+1234567890",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                userManager.Create(seller, "Test1234");
                userManager.AddToRole(seller.Id, "seller");
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
                userManager.Create(customer, "Test1234");
                userManager.AddToRole(customer.Id, "customer");
            }

            context.SaveChanges();

            // Seed Categories
            if (!context.Categories.Any())
            {
                var electronics = new Category { CategoryName = "Electronics" };
                var clothing = new Category { CategoryName = "Clothing" };
                var home = new Category { CategoryName = "Home & Living" };

                context.Categories.AddOrUpdate(c => c.CategoryName, electronics, clothing, home);
                context.SaveChanges();

                // Subcategories
                context.Categories.AddOrUpdate(c => c.CategoryName,
                    new Category { CategoryName = "Smartphones", ParentCategoryId = electronics.CategoryId },
                    new Category { CategoryName = "Laptops", ParentCategoryId = electronics.CategoryId },
                    new Category { CategoryName = "Audio", ParentCategoryId = electronics.CategoryId },
                    new Category { CategoryName = "Men's Clothing", ParentCategoryId = clothing.CategoryId },
                    new Category { CategoryName = "Women's Clothing", ParentCategoryId = clothing.CategoryId },
                    new Category { CategoryName = "Furniture", ParentCategoryId = home.CategoryId },
                    new Category { CategoryName = "Kitchen", ParentCategoryId = home.CategoryId }
                );
                context.SaveChanges();
            }

            // Seed Shop
            var sellerUser = context.Users.FirstOrDefault(u => u.Email == "seller@example.com");
            if (sellerUser != null && !context.Shops.Any())
            {
                var techHub = new Shop
                {
                    SellerId = sellerUser.Id,
                    ShopName = "TechHub Electronics",
                    Description = "Your one-stop shop for electronics and gadgets.",
                    IsApproved = true,
                    AverageRating = 4.5m,
                    TotalSales = 150,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Shops.Add(techHub);
                context.SaveChanges();

                // Seed Products
                var smartphones = context.Categories.FirstOrDefault(c => c.CategoryName == "Smartphones");
                var laptops = context.Categories.FirstOrDefault(c => c.CategoryName == "Laptops");

                if (smartphones != null)
                {
                    context.Products.AddOrUpdate(p => p.Slug,
                        new Product
                        {
                            ShopId = techHub.ShopId,
                            CategoryId = smartphones.CategoryId,
                            ProductName = "Premium Smartphone Pro",
                            Slug = "premium-smartphone-pro",
                            Description = "Experience the ultimate in mobile technology.",
                            Price = 999.99m,
                            StockQuantity = 50,
                            Brand = "TechBrand",
                            AverageRating = 4.8m,
                            TotalReviews = 25,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            ShopId = techHub.ShopId,
                            CategoryId = smartphones.CategoryId,
                            ProductName = "Budget Smartphone Lite",
                            Slug = "budget-smartphone-lite",
                            Description = "Affordable smartphone with essential features.",
                            Price = 299.99m,
                            StockQuantity = 100,
                            Brand = "TechBrand",
                            AverageRating = 4.2m,
                            TotalReviews = 45,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    );
                }

                if (laptops != null)
                {
                    context.Products.AddOrUpdate(p => p.Slug,
                        new Product
                        {
                            ShopId = techHub.ShopId,
                            CategoryId = laptops.CategoryId,
                            ProductName = "Professional Laptop 15",
                            Slug = "professional-laptop-15",
                            Description = "Powerful laptop for professionals.",
                            Price = 1499.99m,
                            StockQuantity = 25,
                            Brand = "TechBrand",
                            AverageRating = 4.9m,
                            TotalReviews = 18,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    );
                }

                context.SaveChanges();

                // Seed Discount
                context.Discounts.AddOrUpdate(d => d.Code,
                    new Discount
                    {
                        ShopId = techHub.ShopId,
                        Code = "WELCOME10",
                        DiscountType = DiscountType.Percentage,
                        Value = 10,
                        MinimumOrderAmount = 50,
                        UsageLimit = 100,
                        PerUserLimit = 1,
                        ExpiresAt = DateTime.UtcNow.AddMonths(3),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }

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
                context.SaveChanges();
            }
        }
    }
}
