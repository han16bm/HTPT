using Microsoft.EntityFrameworkCore.Storage;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;
using netcore.Entities.Persistence;

namespace netcore.Entities.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;

        Roles = new GenericRepository<Role>(context);
        Users = new GenericRepository<User>(context);
        CustomerProfiles = new GenericRepository<CustomerProfile>(context);
        CustomerAddresses = new GenericRepository<CustomerAddress>(context);
        Categories = new GenericRepository<Category>(context);
        Products = new GenericRepository<Product>(context);
        ProductImages = new GenericRepository<ProductImage>(context);
        InventoryTransactions = new GenericRepository<InventoryTransaction>(context);
        Promotions = new GenericRepository<Promotion>(context);
        PromotionProducts = new GenericRepository<PromotionProduct>(context);
        ShoppingCarts = new GenericRepository<ShoppingCart>(context);
        CartItems = new GenericRepository<CartItem>(context);
        Orders = new GenericRepository<Order>(context);
        OrderItems = new GenericRepository<OrderItem>(context);
        Payments = new GenericRepository<Payment>(context);
        BlogCategories = new GenericRepository<BlogCategory>(context);
        BlogPosts = new GenericRepository<BlogPost>(context);
        ContactMessages = new GenericRepository<ContactMessage>(context);
    }

    public IRepository<Role> Roles { get; }
    public IRepository<User> Users { get; }
    public IRepository<CustomerProfile> CustomerProfiles { get; }
    public IRepository<CustomerAddress> CustomerAddresses { get; }
    public IRepository<Category> Categories { get; }
    public IRepository<Product> Products { get; }
    public IRepository<ProductImage> ProductImages { get; }
    public IRepository<InventoryTransaction> InventoryTransactions { get; }
    public IRepository<Promotion> Promotions { get; }
    public IRepository<PromotionProduct> PromotionProducts { get; }
    public IRepository<ShoppingCart> ShoppingCarts { get; }
    public IRepository<CartItem> CartItems { get; }
    public IRepository<Order> Orders { get; }
    public IRepository<OrderItem> OrderItems { get; }
    public IRepository<Payment> Payments { get; }
    public IRepository<BlogCategory> BlogCategories { get; }
    public IRepository<BlogPost> BlogPosts { get; }
    public IRepository<ContactMessage> ContactMessages { get; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
        {
            return;
        }

        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
        {
            return;
        }

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
        }
        _context.Dispose();
    }
}
