using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> entity)
    {
        entity.ToTable("SHOPPING_CARTS");
        entity.HasKey(e => e.Id).HasName("SYS_C008434");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.CustomerId).HasColumnType("NUMBER").HasColumnName("CUSTOMER_ID");
        entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'ACTIVE' ").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");
    }
}
