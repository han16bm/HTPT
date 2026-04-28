using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> entity)
    {
        entity.ToTable("CUSTOMER_PROFILES");
        entity.HasKey(e => e.Id).HasName("SYS_C008365");
        entity.HasIndex(e => e.UserId, "IDX_CUSTOMER_USER_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.UserId).HasColumnType("NUMBER").HasColumnName("USER_ID");
        entity.Property(e => e.CustomerCode).HasMaxLength(30).IsUnicode(false).HasColumnName("CUSTOMER_CODE");
        entity.Property(e => e.FullName).HasMaxLength(150).IsUnicode(false).HasColumnName("FULL_NAME");
        entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false).HasColumnName("EMAIL");
        entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("PHONE");
        entity.Property(e => e.Gender).HasMaxLength(10).IsUnicode(false).HasColumnName("GENDER");
        entity.Property(e => e.DateOfBirth).HasColumnType("DATE").HasColumnName("DATE_OF_BIRTH");
        entity.Property(e => e.Status).HasDefaultValueSql("1 ").HasColumnType("NUMBER(1)").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");
    }
}
