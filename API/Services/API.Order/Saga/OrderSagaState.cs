using MassTransit;
using netcore.Commons.Messages.Events;

namespace API.Order.Saga;

public class OrderSagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }

    public string CurrentState { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public List<OrderItemEventDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
}
