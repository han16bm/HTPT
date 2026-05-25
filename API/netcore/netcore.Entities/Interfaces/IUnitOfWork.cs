using netcore.Entities.Entities;

namespace netcore.Entities.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IRepository<Role> Roles { get; }
    IRepository<User> Users { get; }
    IRepository<CustomerProfile> CustomerProfiles { get; }
    IRepository<CustomerAddress> CustomerAddresses { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<InventoryTransaction> InventoryTransactions { get; }
    IRepository<Promotion> Promotions { get; }
    IRepository<PromotionProduct> PromotionProducts { get; }
    IRepository<ShoppingCart> ShoppingCarts { get; }
    IRepository<CartItem> CartItems { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Payment> Payments { get; }
    IRepository<BlogCategory> BlogCategories { get; }
    IRepository<BlogPost> BlogPosts { get; }
    IRepository<ContactMessage> ContactMessages { get; }

    // Persistence
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
