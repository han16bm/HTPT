using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.ToTable("ORDER_ITEMS");
        entity.HasKey(e => e.Id).HasName("SYS_C008468");
        entity.HasIndex(e => e.OrderId, "IDX_ORDER_ITEMS_ORDER_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.OrderId).HasColumnType("NUMBER").HasColumnName("ORDER_ID");
        entity.Property(e => e.ProductId).HasColumnType("NUMBER").HasColumnName("PRODUCT_ID");
        entity.Property(e => e.ProductName).HasMaxLength(200).IsUnicode(false).HasColumnName("PRODUCT_NAME");
        entity.Property(e => e.Sku).HasMaxLength(50).IsUnicode(false).HasColumnName("SKU");
        entity.Property(e => e.Quantity).HasColumnType("NUMBER").HasColumnName("QUANTITY");
        entity.Property(e => e.UnitPrice).HasColumnType("NUMBER(14,2)").HasColumnName("UNIT_PRICE");
        entity.Property(e => e.DiscountAmount).HasDefaultValueSql("0 ").HasColumnType("NUMBER(14,2)").HasColumnName("DISCOUNT_AMOUNT");
        entity.Property(e => e.LineTotal).HasColumnType("NUMBER(14,2)").HasColumnName("LINE_TOTAL");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");

        // Navigation: OrderItem → Product
        entity.HasOne(oi => oi.Product)
              .WithMany()
              .HasForeignKey(oi => oi.ProductId)
              .HasConstraintName("FK_ORDER_ITEMS_PRODUCT");

        // Navigation: OrderItem → Order (many-to-one)
        entity.HasOne(oi => oi.Order)
              .WithMany(o => o.OrderItems)
              .HasForeignKey(oi => oi.OrderId)
              .HasConstraintName("FK_ORDER_ITEMS_ORDER");
    }
}
