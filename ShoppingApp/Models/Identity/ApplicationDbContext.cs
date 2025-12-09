using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static ApplicationDbContext()
        {
            // Set database initializer to run migrations and seed data
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, ShoppingApp.Migrations.Configuration>());
        }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        // Domain DbSets
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ShopOrder> ShopOrders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewResponse> ReviewResponses { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys
            modelBuilder.Entity<CartItem>()
                .HasKey(c => new { c.UserId, c.ProductId });

            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.ProductId });

            // Category self-referencing relationship
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId);

            // Shop - Seller (One-to-Many from EF perspective, but logically one-to-one)
            modelBuilder.Entity<Shop>()
                .HasRequired(s => s.Seller)
                .WithMany()
                .HasForeignKey(s => s.SellerId)
                .WillCascadeOnDelete(false);

            // Product relationships
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);

            // ProductImage relationship
            modelBuilder.Entity<ProductImage>()
                .HasRequired(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .WillCascadeOnDelete(true);

            // ProductSpecification relationship
            modelBuilder.Entity<ProductSpecification>()
                .HasRequired(ps => ps.Product)
                .WithMany(p => p.Specifications)
                .HasForeignKey(ps => ps.ProductId)
                .WillCascadeOnDelete(true);

            // Address relationship
            modelBuilder.Entity<Address>()
                .HasRequired(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(true);

            // CartItem relationships
            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .WillCascadeOnDelete(false);

            // Favorite relationships
            modelBuilder.Entity<Favorite>()
                .HasRequired(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Favorite>()
                .HasRequired(f => f.Product)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProductId)
                .WillCascadeOnDelete(false);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasOptional(o => o.Discount)
                .WithMany()
                .HasForeignKey(o => o.DiscountId)
                .WillCascadeOnDelete(false);

            // ShopOrder relationships
            modelBuilder.Entity<ShopOrder>()
                .HasRequired(so => so.Order)
                .WithMany(o => o.ShopOrders)
                .HasForeignKey(so => so.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ShopOrder>()
                .HasRequired(so => so.Shop)
                .WithMany(s => s.ShopOrders)
                .HasForeignKey(so => so.ShopId)
                .WillCascadeOnDelete(false);

            // OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.ShopOrder)
                .WithMany(so => so.OrderItems)
                .HasForeignKey(oi => oi.ShopOrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .WillCascadeOnDelete(false);

            // Review relationships
            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId)
                .WillCascadeOnDelete(false);

            // ReviewResponse relationship (One-to-Many from EF perspective)
            modelBuilder.Entity<ReviewResponse>()
                .HasRequired(rr => rr.Review)
                .WithMany()
                .HasForeignKey(rr => rr.ReviewId)
                .WillCascadeOnDelete(true);

            // Discount relationship
            modelBuilder.Entity<Discount>()
                .HasOptional(d => d.Shop)
                .WithMany(s => s.Discounts)
                .HasForeignKey(d => d.ShopId)
                .WillCascadeOnDelete(true);

            // Notification relationship
            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(true);

            // Configure decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<Shop>()
                .Property(s => s.AverageRating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.SubtotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShopOrder>()
                .Property(so => so.ShopSubtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShopOrder>()
                .Property(so => so.ShopTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtPurchase)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Discount>()
                .Property(d => d.Value)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Discount>()
                .Property(d => d.MinimumOrderAmount)
                .HasPrecision(18, 2);
        }
    }
}
