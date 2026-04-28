using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class InventoryTransaction
{
    public decimal Id { get; set; }

    public decimal ProductId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal? UnitCost { get; set; }

    public string? ReferenceType { get; set; }

    public decimal? ReferenceId { get; set; }

    public string? Note { get; set; }

    public decimal? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}


