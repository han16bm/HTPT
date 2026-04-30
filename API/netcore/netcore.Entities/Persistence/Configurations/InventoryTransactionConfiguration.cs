using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> entity)
    {
        entity.ToTable("INVENTORY_TRANSACTIONS");
        entity.HasKey(e => e.Id).HasName("SYS_C008412");
        entity.HasIndex(e => e.ProductId, "IDX_INV_TX_PRODUCT_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.ProductId).HasColumnName("PRODUCT_ID");
        entity.Property(e => e.TransactionType).HasMaxLength(20).IsUnicode(false).HasColumnName("TRANSACTION_TYPE");
        entity.Property(e => e.Quantity).HasColumnName("QUANTITY");
        entity.Property(e => e.UnitCost).HasColumnName("UNIT_COST");
        entity.Property(e => e.ReferenceType).HasMaxLength(30).IsUnicode(false).HasColumnName("REFERENCE_TYPE");
        entity.Property(e => e.ReferenceId).HasColumnName("REFERENCE_ID");
        entity.Property(e => e.Note).HasMaxLength(500).IsUnicode(false).HasColumnName("NOTE");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");

        // Navigation: InventoryTransaction → Product.
        entity.HasOne(t => t.Product)
              .WithMany()
              .HasForeignKey(t => t.ProductId)
              .HasConstraintName("FK_INV_TX_PRODUCT");
    }
}


