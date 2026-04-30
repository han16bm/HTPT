namespace netcore.Commons.Messages.Events;

public record OrderCreatedEvent
{
    public string OrderCode { get; init; } = string.Empty;
    public long? CustomerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<OrderItemEventDto> Items { get; init; } = new();
}

public record OrderItemEventDto
{
    public long ProductId { get; init; }
    public int Quantity { get; init; }
}
