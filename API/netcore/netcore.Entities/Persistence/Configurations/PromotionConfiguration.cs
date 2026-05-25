using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> entity)
    {
        entity.ToTable("PROMOTIONS");
        entity.HasKey(e => e.Id).HasName("SYS_C008425");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.PromoCode).HasMaxLength(50).IsUnicode(false).HasColumnName("PROMO_CODE");
        entity.Property(e => e.Title).HasMaxLength(200).HasColumnName("TITLE");
        entity.Property(e => e.Description).HasMaxLength(1000).HasColumnName("DESCRIPTION");
        entity.Property(e => e.DiscountType).HasMaxLength(20).IsUnicode(false).HasColumnName("DISCOUNT_TYPE");
        entity.Property(e => e.DiscountValue).HasColumnName("DISCOUNT_VALUE");
        entity.Property(e => e.MaxDiscountValue).HasColumnName("MAX_DISCOUNT_VALUE");
        entity.Property(e => e.MinOrderValue).HasDefaultValueSql("0").HasColumnName("MIN_ORDER_VALUE");
        entity.Property(e => e.UsageLimit).HasColumnName("USAGE_LIMIT");
        entity.Property(e => e.UsedCount).HasDefaultValueSql("0").HasColumnName("USED_COUNT");
        entity.Property(e => e.StartAt).HasColumnName("START_AT");
        entity.Property(e => e.EndAt).HasColumnName("END_AT");
        entity.Property(e => e.Status).HasDefaultValueSql("1").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


