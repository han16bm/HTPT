using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        entity.ToTable("PAYMENTS");
        entity.HasKey(e => e.Id).HasName("SYS_C008477");
        entity.HasIndex(e => e.OrderId, "IDX_PAYMENTS_ORDER_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.PaymentCode).HasMaxLength(30).IsUnicode(false).HasColumnName("PAYMENT_CODE");
        entity.Property(e => e.OrderId).HasColumnName("ORDER_ID");
        entity.Property(e => e.Amount).HasColumnName("AMOUNT");
        entity.Property(e => e.Method).HasMaxLength(20).IsUnicode(false).HasColumnName("METHOD");
        entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'PENDING' ").HasColumnName("STATUS");
        entity.Property(e => e.TransactionRef).HasMaxLength(100).IsUnicode(false).HasColumnName("TRANSACTION_REF");
        entity.Property(e => e.PaidAt).HasColumnName("PAID_AT");
        entity.Property(e => e.Note).HasMaxLength(500).IsUnicode(false).HasColumnName("NOTE");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


