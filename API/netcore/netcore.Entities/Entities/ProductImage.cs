using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class ProductImage
{
    public decimal Id { get; set; }

    public decimal ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? AltText { get; set; }

    public bool? IsPrimary { get; set; }

    public decimal DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}


