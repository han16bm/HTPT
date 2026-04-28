using System;

namespace netcore.Entities.Entities;

/// <summary>
/// Partial extension — thêm navigation properties cho User.
/// File gốc User.cs được scaffold từ Oracle, file này bổ sung relationships.
/// </summary>
public partial class User
{
    public bool? IsAdmin { get; set; }      // NUMBER(1) → bool? như tất cả STATUS columns
    public decimal? CreatedBy { get; set; }
    public decimal? UpdatedBy { get; set; }
    public DateTime? UpdatedAt2 { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExp { get; set; }

    // Navigation
    public virtual Role? Role { get; set; }
    public virtual CustomerProfile? CustomerProfile { get; set; }
}


