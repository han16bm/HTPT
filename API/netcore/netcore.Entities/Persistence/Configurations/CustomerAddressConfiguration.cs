using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> entity)
    {
        entity.ToTable("CUSTOMER_ADDRESSES");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.CustomerId, "IDX_CUSTOMER_ADDRESSES_CUSTOMER");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
        entity.Property(e => e.ReceiverName).HasMaxLength(150).HasColumnName("RECEIVER_NAME");
        entity.Property(e => e.ReceiverPhone).HasMaxLength(20).IsUnicode(false).HasColumnName("RECEIVER_PHONE");
        entity.Property(e => e.AddressLine).HasMaxLength(255).HasColumnName("ADDRESS_LINE");
        entity.Property(e => e.Ward).HasMaxLength(100).HasColumnName("WARD");
        entity.Property(e => e.District).HasMaxLength(100).HasColumnName("DISTRICT");
        entity.Property(e => e.Province).HasMaxLength(100).HasColumnName("PROVINCE");
        entity.Property(e => e.IsDefault).HasDefaultValueSql("0").HasColumnName("IS_DEFAULT");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


