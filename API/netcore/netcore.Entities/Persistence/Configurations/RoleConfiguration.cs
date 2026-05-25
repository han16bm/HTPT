using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("ROLES");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.Code).HasMaxLength(30).IsUnicode(false).HasColumnName("CODE");
        entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("NAME");
        entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("DESCRIPTION");
        entity.Property(e => e.Status).HasDefaultValueSql("1").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


