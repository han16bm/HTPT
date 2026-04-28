using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Promotion
{
    public decimal Id { get; set; }

    public string PromoCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountValue { get; set; }

    public decimal MinOrderValue { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public decimal? UsageLimit { get; set; }

    public decimal UsedCount { get; set; }

    public bool? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


