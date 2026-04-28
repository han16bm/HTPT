using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Payment
{
    public decimal Id { get; set; }

    public decimal OrderId { get; set; }

    public string PaymentCode { get; set; } = null!;

    public string Method { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? TransactionRef { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


