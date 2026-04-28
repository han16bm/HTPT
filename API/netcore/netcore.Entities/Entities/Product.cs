using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Product
{
    public decimal Id { get; set; }

    public decimal CategoryId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string? Sku { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal CostPrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal StockQuantity { get; set; }

    public decimal SoldQuantity { get; set; }

    public decimal? WeightGrams { get; set; }

    public bool? Status { get; set; }

    public bool? IsFeatured { get; set; }

    public decimal? CreatedBy { get; set; }

    public decimal? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


