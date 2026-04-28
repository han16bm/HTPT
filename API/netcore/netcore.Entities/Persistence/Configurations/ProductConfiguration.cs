using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.ToTable("PRODUCTS");
        entity.HasKey(e => e.Id).HasName("SYS_C008399");
        entity.HasIndex(e => e.CategoryId, "IDX_PRODUCTS_CATEGORY_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.CategoryId).HasColumnType("NUMBER").HasColumnName("CATEGORY_ID");
        entity.Property(e => e.ProductCode).HasMaxLength(30).IsUnicode(false).HasColumnName("PRODUCT_CODE");
        entity.Property(e => e.Sku).HasMaxLength(50).IsUnicode(false).HasColumnName("SKU");
        entity.Property(e => e.Name).HasMaxLength(200).IsUnicode(false).HasColumnName("NAME");
        entity.Property(e => e.Slug).HasMaxLength(250).IsUnicode(false).HasColumnName("SLUG");
        entity.Property(e => e.Description).HasColumnType("CLOB").HasColumnName("DESCRIPTION");
        entity.Property(e => e.ShortDescription).HasMaxLength(500).IsUnicode(false).HasColumnName("SHORT_DESCRIPTION");
        entity.Property(e => e.ImageUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("IMAGE_URL");
        entity.Property(e => e.CostPrice).HasDefaultValueSql("0 ").HasColumnType("NUMBER(14,2)").HasColumnName("COST_PRICE");
        entity.Property(e => e.SalePrice).HasColumnType("NUMBER(14,2)").HasColumnName("SALE_PRICE");
        entity.Property(e => e.StockQuantity).HasDefaultValueSql("0 ").HasColumnType("NUMBER").HasColumnName("STOCK_QUANTITY");
        entity.Property(e => e.SoldQuantity).HasDefaultValueSql("0 ").HasColumnType("NUMBER").HasColumnName("SOLD_QUANTITY");
        entity.Property(e => e.WeightGrams).HasColumnType("NUMBER(10,2)").HasColumnName("WEIGHT_GRAMS");
        entity.Property(e => e.IsFeatured).HasDefaultValueSql("0 ").HasColumnType("NUMBER(1)").HasColumnName("IS_FEATURED");
        entity.Property(e => e.Status).HasDefaultValueSql("1 ").HasColumnType("NUMBER(1)").HasColumnName("STATUS");
        entity.Property(e => e.CreatedBy).HasColumnType("NUMBER").HasColumnName("CREATED_BY");
        entity.Property(e => e.UpdatedBy).HasColumnType("NUMBER").HasColumnName("UPDATED_BY");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");

        // Navigation: Product → Category (nhiều SP thuộc 1 danh mục).
        entity.HasOne(p => p.Category)
              .WithMany()
              .HasForeignKey(p => p.CategoryId)
              .HasConstraintName("FK_PRODUCTS_CATEGORY");

        // Navigation: Product → ProductImages.
        entity.HasMany(p => p.ProductImages)
              .WithOne()
              .HasForeignKey(i => i.ProductId)
              .HasConstraintName("FK_PRODUCT_IMAGES_PRODUCT");
    }
}
