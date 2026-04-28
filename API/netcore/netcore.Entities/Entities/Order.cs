using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Order
{
    public decimal Id { get; set; }

    public string OrderCode { get; set; } = null!;

    public decimal? CustomerId { get; set; }

    public decimal? AddressId { get; set; }

    public decimal? PromotionId { get; set; }

    public string OrderSource { get; set; } = null!;

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public string CustomerAddress { get; set; } = null!;

    public string? Note { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? CreatedBy { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


