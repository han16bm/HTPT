using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> entity)
    {
        entity.ToTable("BLOG_POSTS");
        entity.HasKey(e => e.Id).HasName("SYS_C008492");
        entity.HasIndex(e => e.AuthorId, "IDX_BLOG_POSTS_AUTHOR_ID");
        entity.HasIndex(e => e.CategoryId, "IDX_BLOG_POSTS_CATEGORY_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.CategoryId).HasColumnName("CATEGORY_ID");
        entity.Property(e => e.AuthorId).HasColumnName("AUTHOR_ID");
        entity.Property(e => e.Title).HasMaxLength(300).HasColumnName("TITLE");
        entity.Property(e => e.Slug).HasMaxLength(300).IsUnicode(false).HasColumnName("SLUG");
        entity.Property(e => e.Summary).HasMaxLength(1000).HasColumnName("SUMMARY");
        entity.Property(e => e.Content).HasColumnName("CONTENT");
        entity.Property(e => e.ThumbnailUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("THUMBNAIL_URL");
        entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'DRAFT' ").HasColumnName("STATUS");
        entity.Property(e => e.PublishedAt).HasColumnName("PUBLISHED_AT");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");

        // Navigation: BlogPost → BlogCategory
        entity.HasOne(bp => bp.BlogCategory)
              .WithMany()
              .HasForeignKey(bp => bp.CategoryId)
              .HasConstraintName("FK_BLOG_POSTS_CATEGORY");
    }
}


