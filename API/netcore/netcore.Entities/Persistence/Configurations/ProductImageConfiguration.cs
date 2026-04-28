using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> entity)
    {
        entity.ToTable("PRODUCT_IMAGES");
        entity.HasKey(e => e.Id).HasName("SYS_C008406");
        entity.HasIndex(e => e.ProductId, "IDX_PRODUCT_IMAGES_PRODUCT_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.ProductId).HasColumnType("NUMBER").HasColumnName("PRODUCT_ID");
        entity.Property(e => e.ImageUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("IMAGE_URL");
        entity.Property(e => e.AltText).HasMaxLength(255).IsUnicode(false).HasColumnName("ALT_TEXT");
        entity.Property(e => e.IsPrimary).HasDefaultValueSql("0 ").HasColumnType("NUMBER(1)").HasColumnName("IS_PRIMARY");
        entity.Property(e => e.DisplayOrder).HasDefaultValueSql("0 ").HasColumnType("NUMBER").HasColumnName("DISPLAY_ORDER");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
    }
}
