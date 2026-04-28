using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class User
{
    public decimal Id { get; set; }

    public decimal RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public bool? Status { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


