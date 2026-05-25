using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> entity)
    {
        entity.ToTable("BLOG_CATEGORIES");
        entity.HasKey(e => e.Id).HasName("SYS_C008484");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.Name).HasMaxLength(150).HasColumnName("NAME");
        entity.Property(e => e.Slug).HasMaxLength(200).IsUnicode(false).HasColumnName("SLUG");
        entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("DESCRIPTION");
        entity.Property(e => e.Status).HasDefaultValueSql("1").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


