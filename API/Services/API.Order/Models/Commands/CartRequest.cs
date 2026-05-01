using System.ComponentModel.DataAnnotations;

namespace API.Order.Models.Commands;

public class AddToCartRequest
{
    [Required]
    public long ProductId { get; set; }

    [Required, Range(1, 999)]
    public int Quantity { get; set; } = 1;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public decimal UnitPrice { get; set; }

    public string? ImageUrl { get; set; }
}

public class UpdateCartItemRequest
{
    [Required]
    public long CartItemId { get; set; }

    [Required, Range(1, 999)]
    public int Quantity { get; set; }
}

public class RemoveCartItemRequest
{
    [Required]
    public long CartItemId { get; set; }
}
