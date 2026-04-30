using netcore.Commons.Contracts.Events;

namespace API.Order.Messages;

public sealed record OrderCreatedMessage
{
    public required string OrderCode { get; init; }
    public long? UserId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<OrderCreatedItemMessage> Items { get; init; } = [];

    public OrderCreatedEvent ToEvent() => new()
    {
        OrderCode = OrderCode,
        UserId = UserId,
        TotalAmount = TotalAmount,
        CreatedAt = CreatedAt,
        Items = Items.Select(item => new OrderCreatedItemEvent
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList()
    };
}

public sealed record OrderCreatedItemMessage
{
    public required long ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
