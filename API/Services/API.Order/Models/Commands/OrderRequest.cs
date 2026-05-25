using System.ComponentModel.DataAnnotations;

namespace API.Order.Models.Commands;

public class CreateOrderRequest
{
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public string? ShippingAddress { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "COD";

    public string? PromotionCode { get; set; }

    public decimal ShippingFee { get; set; } = 0;

    public string? Note { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    public string OrderCode { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty; // PENDING|CONFIRMED|SHIPPING|COMPLETED|CANCELLED

    public string? Note { get; set; }
}

public class CancelOrderRequest
{
    [Required]
    public string OrderCode { get; set; } = string.Empty;

    public string? Reason { get; set; }
}

// Đặt hàng trực tiếp (không qua giỏ) — POS hoặc admin
public class DirectOrderRequest
{
    public long? CustomerId { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }

    [Required]
    public List<OrderLineRequest> Items { get; set; } = [];

    public string PaymentMethod { get; set; } = "CASH";
    public string? PromoCode { get; set; }
    public decimal ShippingFee { get; set; } = 0;
    public string? Note { get; set; }
    public string Source { get; set; } = "POS";
}

public class OrderLineRequest
{
    [Required]
    public long ProductId { get; set; }

    [Required, Range(1, 9999)]
    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }
}
