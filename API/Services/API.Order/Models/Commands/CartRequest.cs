using System.ComponentModel.DataAnnotations;

namespace API.Order.Models.Commands;

public class AddToCartRequest
{
    [Required]
    public long ProductId { get; set; }

    [Required, Range(1, 999)]
    public int Quantity { get; set; } = 1;
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
