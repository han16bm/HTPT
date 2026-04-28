using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> entity)
    {
        entity.ToTable("CUSTOMER_ADDRESSES");
        entity.HasKey(e => e.Id).HasName("SYS_C008376");
        entity.HasIndex(e => e.CustomerId, "IDX_CUSTOMER_ADDRESSES_CUSTOMER");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.CustomerId).HasColumnType("NUMBER").HasColumnName("CUSTOMER_ID");
        entity.Property(e => e.ReceiverName).HasMaxLength(150).IsUnicode(false).HasColumnName("RECEIVER_NAME");
        entity.Property(e => e.ReceiverPhone).HasMaxLength(20).IsUnicode(false).HasColumnName("RECEIVER_PHONE");
        entity.Property(e => e.AddressLine).HasMaxLength(255).IsUnicode(false).HasColumnName("ADDRESS_LINE");
        entity.Property(e => e.Ward).HasMaxLength(100).IsUnicode(false).HasColumnName("WARD");
        entity.Property(e => e.District).HasMaxLength(100).IsUnicode(false).HasColumnName("DISTRICT");
        entity.Property(e => e.Province).HasMaxLength(100).IsUnicode(false).HasColumnName("PROVINCE");
        entity.Property(e => e.IsDefault).HasDefaultValueSql("0 ").HasColumnType("NUMBER(1)").HasColumnName("IS_DEFAULT");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");
    }
}
