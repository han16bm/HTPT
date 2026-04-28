using Microsoft.EntityFrameworkCore;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence;

/// <summary>
/// EF Core DbContext cho FISH_SHOP — map với Oracle schema FSHOP_DB.
/// Cấu hình cột/khóa/quan hệ được tách sang <see cref="IEntityTypeConfiguration{T}"/>
/// trong thư mục <c>Persistence/Configurations/</c> (1 file / entity).
/// Kết nối được nạp từ <c>IConfiguration</c> qua <c>EntityServicesExtensions</c>;
/// tuyệt đối không hard-code trong <see cref="OnConfiguring"/>.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public virtual DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public virtual DbSet<CartItem> CartItems => Set<CartItem>();
    public virtual DbSet<Category> Categories => Set<Category>();
    public virtual DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public virtual DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public virtual DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public virtual DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public virtual DbSet<Order> Orders => Set<Order>();
    public virtual DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public virtual DbSet<Payment> Payments => Set<Payment>();
    public virtual DbSet<Product> Products => Set<Product>();
    public virtual DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public virtual DbSet<Promotion> Promotions => Set<Promotion>();
    public virtual DbSet<PromotionProduct> PromotionProducts => Set<PromotionProduct>();
    public virtual DbSet<Role> Roles => Set<Role>();
    public virtual DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public virtual DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("FSHOP_DB")
            .UseCollation("USING_NLS_COMP");

        // Auto-pick mọi IEntityTypeConfiguration<T> nằm trong assembly này.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
