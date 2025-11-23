using ShoppingApp.Core.Entities;

namespace ShoppingApp.Core.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<Address> Addresses { get; }
    IRepository<Shop> Shops { get; }
    IRepository<Category> Categories { get; }
    IProductRepository Products { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<CartItem> CartItems { get; }
    IRepository<Favorite> Favorites { get; }
    IRepository<Order> Orders { get; }
    IRepository<ShopOrder> ShopOrders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Review> Reviews { get; }
    IRepository<Discount> Discounts { get; }
    IRepository<Notification> Notifications { get; }

    Task<int> SaveChangesAsync();
}
