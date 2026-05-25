using System;

namespace netcore.Entities.Entities;

public partial class User
{
    public bool? IsAdmin { get; set; }
    public decimal? CreatedBy { get; set; }
    public decimal? UpdatedBy { get; set; }
    public DateTime? UpdatedAt2 { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExp { get; set; }

    public virtual Role? Role { get; set; }
    public virtual CustomerProfile? CustomerProfile { get; set; }
}
