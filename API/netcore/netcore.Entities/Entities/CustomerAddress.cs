using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class CustomerAddress
{
    public decimal Id { get; set; }

    public decimal CustomerId { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string ReceiverPhone { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string District { get; set; } = null!;

    public string? Ward { get; set; }

    public string AddressLine { get; set; } = null!;

    public bool? IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


