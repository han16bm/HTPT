using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> entity)
    {
        entity.ToTable("CUSTOMER_PROFILES");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.UserId, "IDX_CUSTOMER_USER_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.UserId).HasColumnName("USER_ID");
        entity.Property(e => e.CustomerCode).HasMaxLength(30).IsUnicode(false).HasColumnName("CUSTOMER_CODE");
        entity.Property(e => e.FullName).HasMaxLength(150).IsUnicode(false).HasColumnName("FULL_NAME");
        entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false).HasColumnName("EMAIL");
        entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("PHONE");
        entity.Property(e => e.Gender).HasMaxLength(10).IsUnicode(false).HasColumnName("GENDER");
        entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
        entity.Property(e => e.Status).HasDefaultValueSql("1").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


