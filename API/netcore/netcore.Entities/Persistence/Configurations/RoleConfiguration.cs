using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("ROLES");
        entity.HasKey(e => e.Id).HasName("SYS_C008349");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.Code).HasMaxLength(30).IsUnicode(false).HasColumnName("CODE");
        entity.Property(e => e.Name).HasMaxLength(100).IsUnicode(false).HasColumnName("NAME");
        entity.Property(e => e.Description).HasMaxLength(500).IsUnicode(false).HasColumnName("DESCRIPTION");
        entity.Property(e => e.Status).HasDefaultValueSql("1 ").HasColumnType("NUMBER(1)").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");
    }
}
