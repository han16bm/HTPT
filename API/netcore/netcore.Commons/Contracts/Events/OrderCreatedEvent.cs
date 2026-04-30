namespace netcore.Commons.Contracts.Events;

public sealed record OrderCreatedEvent
{
    public required string OrderCode { get; init; }
    public long? UserId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<OrderCreatedItemEvent> Items { get; init; } = [];
}

public sealed record OrderCreatedItemEvent
{
    public required long ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
