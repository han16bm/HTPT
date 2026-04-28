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

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("NUMBER").HasColumnName("ID");
        entity.Property(e => e.PromoCode).HasMaxLength(30).IsUnicode(false).HasColumnName("PROMO_CODE");
        entity.Property(e => e.Title).HasMaxLength(200).IsUnicode(false).HasColumnName("TITLE");
        entity.Property(e => e.Description).HasMaxLength(1000).IsUnicode(false).HasColumnName("DESCRIPTION");
        entity.Property(e => e.DiscountType).HasMaxLength(20).IsUnicode(false).HasColumnName("DISCOUNT_TYPE");
        entity.Property(e => e.DiscountValue).HasColumnType("NUMBER(14,2)").HasColumnName("DISCOUNT_VALUE");
        entity.Property(e => e.MaxDiscountValue).HasColumnType("NUMBER(14,2)").HasColumnName("MAX_DISCOUNT_VALUE");
        entity.Property(e => e.MinOrderValue).HasDefaultValueSql("0 ").HasColumnType("NUMBER(14,2)").HasColumnName("MIN_ORDER_VALUE");
        entity.Property(e => e.UsageLimit).HasColumnType("NUMBER").HasColumnName("USAGE_LIMIT");
        entity.Property(e => e.UsedCount).HasDefaultValueSql("0 ").HasColumnType("NUMBER").HasColumnName("USED_COUNT");
        entity.Property(e => e.StartAt).HasColumnType("DATE").HasColumnName("START_AT");
        entity.Property(e => e.EndAt).HasColumnType("DATE").HasColumnName("END_AT");
        entity.Property(e => e.Status).HasDefaultValueSql("1 ").HasColumnType("NUMBER(1)").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSDATE ").HasColumnType("DATE").HasColumnName("UPDATED_AT");
    }
}
