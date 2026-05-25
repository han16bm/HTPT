using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using netcore.Entities.Entities;

namespace netcore.Entities.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("USERS");
        entity.HasKey(e => e.Id).HasName("PK_USERS");
        entity.HasIndex(e => e.RoleId, "IDX_USERS_ROLE_ID");

        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("ID");
        entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
        entity.Property(e => e.Username).HasMaxLength(50).IsUnicode(false).HasColumnName("USERNAME");
        entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false).HasColumnName("EMAIL");
        entity.Property(e => e.PasswordHash).HasMaxLength(255).IsUnicode(false).HasColumnName("PASSWORD_HASH");
        entity.Property(e => e.FullName).HasMaxLength(150).HasColumnName("FULL_NAME");
        entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("PHONE");
        entity.Property(e => e.AvatarUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("AVATAR_URL");
        entity.Property(e => e.Status).HasDefaultValueSql("1").HasColumnName("STATUS");
        entity.Property(e => e.LastLoginAt).HasColumnName("LAST_LOGIN_AT");
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("CREATED_AT");
        entity.Property(e => e.UpdatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("SYSUTCDATETIME()").HasColumnName("UPDATED_AT");

        // Các cột được bổ sung qua partial (xem UserExtensions.cs).
        entity.Property(e => e.IsAdmin).HasDefaultValueSql("0").HasColumnName("IS_ADMIN");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY");
        entity.Property(e => e.ResetToken).HasMaxLength(200).IsUnicode(false).HasColumnName("RESET_TOKEN");
        entity.Property(e => e.ResetTokenExp).HasColumnName("RESET_TOKEN_EXP");

        // UpdatedAt2 chỉ là alias ở code, không map sang cột.
        entity.Ignore(u => u.UpdatedAt2);

        // Navigation: User → Role (nhiều user thuộc 1 role).
        entity.HasOne(u => u.Role)
              .WithMany(r => r.Users)
              .HasForeignKey(u => u.RoleId)
              .HasConstraintName("FK_USERS_ROLE");

        // Navigation: User → CustomerProfile (1-1 optional).
        entity.HasOne(u => u.CustomerProfile)
              .WithOne(cp => cp.User)
              .HasForeignKey<CustomerProfile>(cp => cp.UserId)
              .HasConstraintName("FK_CUSTOMER_PROFILE_USER");
    }
}


