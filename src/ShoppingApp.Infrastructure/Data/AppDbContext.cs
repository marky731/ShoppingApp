using Microsoft.EntityFrameworkCore;
using ShoppingApp.Core.Entities;

namespace ShoppingApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ShopOrder> ShopOrders => Set<ShopOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewResponse> ReviewResponses => Set<ReviewResponse>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply snake_case naming convention for columns only (tables use PascalCase)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Column names only
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.RoleName).IsUnique();
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId);
        });

        // Address
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId);
            entity.Property(e => e.AddressLabel).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StreetAddress).HasMaxLength(255).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Shop
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId);
            entity.Property(e => e.ShopName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.LogoImageUrl).HasMaxLength(255);
            entity.Property(e => e.BannerImageUrl).HasMaxLength(255);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);

            entity.HasOne(e => e.Seller)
                .WithOne(u => u.Shop)
                .HasForeignKey<Shop>(e => e.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SellerId).IsUnique();
        });

        // Category (self-referencing)
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.MainImageUrl).HasMaxLength(255);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);

            entity.HasOne(e => e.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId);

            // Indexes
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.Brand);
            entity.HasIndex(e => e.IsActive);
        });

        // ProductImage
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);
            entity.Property(e => e.ImageUrl).HasMaxLength(255).IsRequired();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem (composite key)
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ProductId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Favorite (composite key)
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ProductId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Favorites)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Discount
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.DiscountId);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.DiscountType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Value).HasPrecision(10, 2);
            entity.Property(e => e.MinimumOrderAmount).HasPrecision(10, 2);

            entity.HasOne(e => e.Shop)
                .WithMany(s => s.Discounts)
                .HasForeignKey(e => e.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.SubtotalAmount).HasPrecision(10, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
            entity.Property(e => e.PaymentId).HasMaxLength(255);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.ShippingAddress)
                .WithMany()
                .HasForeignKey(e => e.ShippingAddressId);

            entity.HasOne(e => e.Discount)
                .WithMany()
                .HasForeignKey(e => e.DiscountId);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrderDate);
        });

        // ShopOrder
        modelBuilder.Entity<ShopOrder>(entity =>
        {
            entity.HasKey(e => e.ShopOrderId);
            entity.Property(e => e.ShopOrderStatus).HasMaxLength(20);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.ShopSubtotal).HasPrecision(10, 2);
            entity.Property(e => e.ShopTotal).HasPrecision(10, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.ShopOrders)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Shop)
                .WithMany(s => s.ShopOrders)
                .HasForeignKey(e => e.ShopId);

            entity.HasOne(e => e.Discount)
                .WithMany()
                .HasForeignKey(e => e.DiscountId);

            entity.HasIndex(e => e.ShopOrderStatus);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId);
            entity.Property(e => e.PriceAtPurchase).HasPrecision(10, 2);

            entity.HasOne(e => e.ShopOrder)
                .WithMany(so => so.OrderItems)
                .HasForeignKey(e => e.ShopOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId);
        });

        // Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(e => e.OrderId);

            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Status);
        });

        // ReviewResponse
        modelBuilder.Entity<ReviewResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId);

            entity.HasOne(e => e.Review)
                .WithOne(r => r.Response)
                .HasForeignKey<ReviewResponse>(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Seller)
                .WithMany()
                .HasForeignKey(e => e.SellerId);

            entity.HasIndex(e => e.ReviewId).IsUnique();
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.LinkUrl).HasMaxLength(255);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "customer" },
            new Role { RoleId = 2, RoleName = "seller" },
            new Role { RoleId = 3, RoleName = "admin" }
        );
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) result.Append('_');
                result.Append(char.ToLower(c));
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
}
