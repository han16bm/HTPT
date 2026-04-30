using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.ToTable("ORDERS");
        entity.HasKey(e => e.Id).HasName("SYS_C008458");
        entity.HasIndex(e => e.CustomerId, "IDX_ORDERS_CUSTOMER_ID");
        entity.HasIndex(e => e.OrderStatus, "IDX_ORDERS_STATUS");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.OrderCode).HasMaxLength(30).IsUnicode(false).HasColumnName("ORDER_CODE");
        entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
        entity.Property(e => e.CustomerName).HasMaxLength(150).IsUnicode(false).HasColumnName("CUSTOMER_NAME");
        entity.Property(e => e.CustomerPhone).HasMaxLength(20).IsUnicode(false).HasColumnName("CUSTOMER_PHONE");
        entity.Property(e => e.CustomerEmail).HasMaxLength(150).IsUnicode(false).HasColumnName("CUSTOMER_EMAIL");
        entity.Property(e => e.CustomerAddress).HasMaxLength(500).IsUnicode(false).HasColumnName("CUSTOMER_ADDRESS");
        entity.Property(e => e.AddressId).HasColumnName("ADDRESS_ID");
        entity.Property(e => e.SubtotalAmount).HasDefaultValueSql("0").HasColumnName("SUBTOTAL_AMOUNT");
        entity.Property(e => e.DiscountAmount).HasDefaultValueSql("0").HasColumnName("DISCOUNT_AMOUNT");
        entity.Property(e => e.ShippingFee).HasDefaultValueSql("0").HasColumnName("SHIPPING_FEE");
        entity.Property(e => e.TotalAmount).HasDefaultValueSql("0").HasColumnName("TOTAL_AMOUNT");
        entity.Property(e => e.PromotionId).HasColumnName("PROMOTION_ID");
        entity.Property(e => e.PaymentMethod).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'COD' ").HasColumnName("PAYMENT_METHOD");
        entity.Property(e => e.PaymentStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'UNPAID' ").HasColumnName("PAYMENT_STATUS");
        entity.Property(e => e.OrderStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'PENDING' ").HasColumnName("ORDER_STATUS");
        entity.Property(e => e.OrderSource).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'ONLINE' ").HasColumnName("ORDER_SOURCE");
        entity.Property(e => e.Note).HasMaxLength(1000).IsUnicode(false).HasColumnName("NOTE");
        entity.Property(e => e.ConfirmedAt).HasColumnName("CONFIRMED_AT");
        entity.Property(e => e.ShippedAt).HasColumnName("SHIPPED_AT");
        entity.Property(e => e.DeliveredAt).HasColumnName("DELIVERED_AT");
        entity.Property(e => e.CancelledAt).HasColumnName("CANCELLED_AT");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


