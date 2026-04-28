using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class OrderItem
{
    public decimal Id { get; set; }

    public decimal OrderId { get; set; }

    public decimal ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Sku { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineTotal { get; set; }

    public DateTime CreatedAt { get; set; }
}


