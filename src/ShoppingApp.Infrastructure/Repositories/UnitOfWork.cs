using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<Address>? _addresses;
    private IRepository<Shop>? _shops;
    private IRepository<Category>? _categories;
    private IProductRepository? _products;
    private IRepository<ProductImage>? _productImages;
    private IRepository<CartItem>? _cartItems;
    private IRepository<Favorite>? _favorites;
    private IRepository<Order>? _orders;
    private IRepository<ShopOrder>? _shopOrders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<Review>? _reviews;
    private IRepository<Discount>? _discounts;
    private IRepository<Notification>? _notifications;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<Address> Addresses => _addresses ??= new Repository<Address>(_context);
    public IRepository<Shop> Shops => _shops ??= new Repository<Shop>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IRepository<ProductImage> ProductImages => _productImages ??= new Repository<ProductImage>(_context);
    public IRepository<CartItem> CartItems => _cartItems ??= new Repository<CartItem>(_context);
    public IRepository<Favorite> Favorites => _favorites ??= new Repository<Favorite>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
    public IRepository<ShopOrder> ShopOrders => _shopOrders ??= new Repository<ShopOrder>(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);
    public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);
    public IRepository<Discount> Discounts => _discounts ??= new Repository<Discount>(_context);
    public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
