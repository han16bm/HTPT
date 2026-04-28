using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class BlogCategory
{
    public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


