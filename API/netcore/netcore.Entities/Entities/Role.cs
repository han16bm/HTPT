using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Role
{
    public decimal Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


