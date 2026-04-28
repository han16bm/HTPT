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

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.Name).HasMaxLength(150).IsUnicode(false).HasColumnName("NAME");
        entity.Property(e => e.Slug).HasMaxLength(200).IsUnicode(false).HasColumnName("SLUG");
        entity.Property(e => e.Description).HasMaxLength(500).IsUnicode(false).HasColumnName("DESCRIPTION");
        entity.Property(e => e.Status).HasDefaultValueSql("1 ").HasColumnType("NUMBER(1)").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");
    }
}
