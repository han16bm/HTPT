using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> entity)
    {
        entity.ToTable("CONTACT_MESSAGES");
        entity.HasKey(e => e.Id).HasName("SYS_C008499");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.FullName).HasMaxLength(150).HasColumnName("FULL_NAME");
        entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false).HasColumnName("EMAIL");
        entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("PHONE");
        entity.Property(e => e.Subject).HasMaxLength(200).HasColumnName("SUBJECT");
        entity.Property(e => e.Message).HasColumnName("MESSAGE");
        entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValueSql("'NEW' ").HasColumnName("STATUS");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");
    }
}


