using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Category
{
    public decimal Id { get; set; }

    public decimal? ParentId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal DisplayOrder { get; set; }

    public bool? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


