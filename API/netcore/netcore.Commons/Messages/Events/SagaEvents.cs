namespace netcore.Commons.Messages.Events;

// Inventory
public record InventoryReservationRequested
{
    public string OrderCode { get; init; } = string.Empty;
    public List<OrderItemEventDto> Items { get; init; } = new();
}

public record InventoryReservedEvent
{
    public string OrderCode { get; init; } = string.Empty;
}

public record InventoryReservationFailedEvent
{
    public string OrderCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record InventoryReleaseRequested
{
    public string OrderCode { get; init; } = string.Empty;
    public List<OrderItemEventDto> Items { get; init; } = new();
}

public record InventoryReleasedEvent
{
    public string OrderCode { get; init; } = string.Empty;
}

// Payment
public record PaymentProcessRequested
{
    public string OrderCode { get; init; } = string.Empty;
    public long? CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}

public record PaymentProcessedEvent
{
    public string OrderCode { get; init; } = string.Empty;
    public string TransactionRef { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}

public record PaymentFailedEvent
{
    public string OrderCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

// Saga outcome
public record OrderCompletedEvent
{
    public string OrderCode { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
}

public record OrderFailedEvent
{
    public string OrderCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}
