using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.ToTable("CATEGORIES");
        entity.HasKey(e => e.Id).HasName("SYS_C008385");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.CategoryCode).HasMaxLength(30).IsUnicode(false).HasColumnName("CATEGORY_CODE");
        entity.Property(e => e.Name).HasMaxLength(150).HasColumnName("NAME");
        entity.Property(e => e.Slug).HasMaxLength(200).IsUnicode(false).HasColumnName("SLUG");
        entity.Property(e => e.Description).HasMaxLength(1000).HasColumnName("DESCRIPTION");
        entity.Property(e => e.ImageUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("IMAGE_URL");
        entity.Property(e => e.ParentId).HasColumnName("PARENT_ID");
        entity.Property(e => e.DisplayOrder).HasDefaultValueSql("0").HasColumnName("DISPLAY_ORDER");
        entity.Property(e => e.Status).HasDefaultValueSql("1").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


