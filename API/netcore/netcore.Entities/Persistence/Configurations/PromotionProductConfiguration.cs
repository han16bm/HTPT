using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class PromotionProductConfiguration : IEntityTypeConfiguration<PromotionProduct>
{
    public void Configure(EntityTypeBuilder<PromotionProduct> entity)
    {
        entity.ToTable("PROMOTION_PRODUCTS");
        entity.HasKey(e => new { e.PromotionId, e.ProductId }).HasName("SYS_C008428");

        entity.Property(e => e.PromotionId).HasColumnType("NUMBER").HasColumnName("PROMOTION_ID");
        entity.Property(e => e.ProductId).HasColumnType("NUMBER").HasColumnName("PRODUCT_ID");
    }
}
