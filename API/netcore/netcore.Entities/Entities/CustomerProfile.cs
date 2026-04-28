using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class CustomerProfile
{
    public decimal Id { get; set; }

    public decimal? UserId { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public bool? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


