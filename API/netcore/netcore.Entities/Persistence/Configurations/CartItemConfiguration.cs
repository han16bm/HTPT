using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> entity)
    {
        entity.ToTable("CART_ITEMS");
        entity.HasKey(e => e.Id).HasName("SYS_C008442");
        entity.HasIndex(e => e.CartId, "IDX_CART_ITEMS_CART_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.CartId).HasColumnType("NUMBER").HasColumnName("CART_ID");
        entity.Property(e => e.ProductId).HasColumnType("NUMBER").HasColumnName("PRODUCT_ID");
        entity.Property(e => e.Quantity).HasColumnType("NUMBER").HasColumnName("QUANTITY");
        entity.Property(e => e.UnitPrice).HasColumnType("NUMBER(14,2)").HasColumnName("UNIT_PRICE");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");

        // Navigation: CartItem → Product.
        entity.HasOne(ci => ci.Product)
              .WithMany()
              .HasForeignKey(ci => ci.ProductId)
              .HasConstraintName("FK_CART_ITEMS_PRODUCT");
    }
}
