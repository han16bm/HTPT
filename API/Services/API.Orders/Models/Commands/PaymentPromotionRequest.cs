using System.ComponentModel.DataAnnotations;

namespace API.Orders.Models.Commands;

public class CreatePaymentRequest
{
    [Required]
    public long OrderId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "COD";

    public string? TransactionCode { get; set; }
    public string? Notes { get; set; }
}

public class ValidatePromotionRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required, Range(0, double.MaxValue)]
    public decimal OrderAmount { get; set; }
}

public class UpsertPromotionRequest
{
    public long? Id { get; set; }

    [Required, MaxLength(50)]
    public string PromoCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string DiscountType { get; set; } = "PERCENT"; // PERCENT | AMOUNT

    [Required, Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountValue { get; set; }
    public int? UsageLimit { get; set; }
    public int Status { get; set; } = 1;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}
