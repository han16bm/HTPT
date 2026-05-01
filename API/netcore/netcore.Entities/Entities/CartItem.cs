using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class CartItem
{
    public decimal Id { get; set; }

    public decimal CartId { get; set; }

    public decimal ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? ProductName { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


