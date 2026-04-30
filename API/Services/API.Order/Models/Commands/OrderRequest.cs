using System.ComponentModel.DataAnnotations;

namespace API.Order.Models.Commands;

public class CreateOrderRequest
{
    /// <summary>Địa chỉ giao hàng (JSON hoặc chuỗi)</summary>
    [Required]
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>Phương thức thanh toán: COD | BANK_TRANSFER | CASH</summary>
    [Required]
    public string PaymentMethod { get; set; } = "COD";

    /// <summary>Mã giảm giá (tùy chọn)</summary>
    public string? PromotionCode { get; set; }

    /// <summary>Phí vận chuyển</summary>
    public decimal ShippingFee { get; set; } = 0;

    /// <summary>Ghi chú đơn hàng</summary>
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

/// <summary>Đặt hàng trực tiếp (không qua giỏ) — dùng cho POS hoặc admin</summary>
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

    /// <summary>Đơn giá (tùy chọn, nếu null lấy giá sản phẩm)</summary>
    public decimal? UnitPrice { get; set; }
}
