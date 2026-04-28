using Microsoft.EntityFrameworkCore.Storage;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;
using netcore.Entities.Persistence;

namespace netcore.Entities.Repositories;

/// <summary>
/// Unit of Work — kết hợp tất cả repositories, gọi SaveChangesAsync một lần.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-init repositories
    private IRepository<Role>? _roles;
    private IRepository<User>? _users;
    private IRepository<CustomerProfile>? _customerProfiles;
    private IRepository<CustomerAddress>? _customerAddresses;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<ProductImage>? _productImages;
    private IRepository<InventoryTransaction>? _inventoryTransactions;
    private IRepository<Promotion>? _promotions;
    private IRepository<PromotionProduct>? _promotionProducts;
    private IRepository<ShoppingCart>? _shoppingCarts;
    private IRepository<CartItem>? _cartItems;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<Payment>? _payments;
    private IRepository<BlogCategory>? _blogCategories;
    private IRepository<BlogPost>? _blogPosts;
    private IRepository<ContactMessage>? _contactMessages;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Role> Roles => _roles ??= new GenericRepository<Role>(_context);
    public IRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IRepository<CustomerProfile> CustomerProfiles => _customerProfiles ??= new GenericRepository<CustomerProfile>(_context);
    public IRepository<CustomerAddress> CustomerAddresses => _customerAddresses ??= new GenericRepository<CustomerAddress>(_context);
    public IRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IRepository<Product> Products => _products ??= new GenericRepository<Product>(_context);
    public IRepository<ProductImage> ProductImages => _productImages ??= new GenericRepository<ProductImage>(_context);
    public IRepository<InventoryTransaction> InventoryTransactions => _inventoryTransactions ??= new GenericRepository<InventoryTransaction>(_context);
    public IRepository<Promotion> Promotions => _promotions ??= new GenericRepository<Promotion>(_context);
    public IRepository<PromotionProduct> PromotionProducts => _promotionProducts ??= new GenericRepository<PromotionProduct>(_context);
    public IRepository<ShoppingCart> ShoppingCarts => _shoppingCarts ??= new GenericRepository<ShoppingCart>(_context);
    public IRepository<CartItem> CartItems => _cartItems ??= new GenericRepository<CartItem>(_context);
    public IRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new GenericRepository<OrderItem>(_context);
    public IRepository<Payment> Payments => _payments ??= new GenericRepository<Payment>(_context);
    public IRepository<BlogCategory> BlogCategories => _blogCategories ??= new GenericRepository<BlogCategory>(_context);
    public IRepository<BlogPost> BlogPosts => _blogPosts ??= new GenericRepository<BlogPost>(_context);
    public IRepository<ContactMessage> ContactMessages => _contactMessages ??= new GenericRepository<ContactMessage>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
