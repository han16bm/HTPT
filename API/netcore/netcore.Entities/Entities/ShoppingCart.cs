using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class ShoppingCart
{
    public decimal Id { get; set; }

    public decimal CustomerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


