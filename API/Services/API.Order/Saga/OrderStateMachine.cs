using MassTransit;
using netcore.Commons.Messages.Events;

namespace API.Order.Saga;

public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
{
    public State AwaitingInventory { get; private set; } = null!;
    public State AwaitingPayment { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<OrderCreatedEvent> OrderCreated { get; private set; } = null!;
    public Event<InventoryReservedEvent> InventoryReserved { get; private set; } = null!;
    public Event<InventoryReservationFailedEvent> InventoryReservationFailed { get; private set; } = null!;
    public Event<PaymentProcessedEvent> PaymentProcessed { get; private set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = null!;
    public Event<InventoryReleasedEvent> InventoryReleased { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(state => state.CurrentState);

        ConfigureEventCorrelation();
        ConfigureInitialState();
        ConfigureAwaitingInventoryState();
        ConfigureAwaitingPaymentState();
        ConfigureCompensatingState();

        SetCompletedWhenFinalized();
    }

    private void ConfigureEventCorrelation()
    {
        Event(() => OrderCreated, config => config
            .CorrelateBy<string>(saga => saga.OrderCode, ctx => ctx.Message.OrderCode)
            .SelectId(_ => Guid.NewGuid()));

        Event(() => InventoryReserved, config => config
            .CorrelateBy<string>(saga => saga.OrderCode, ctx => ctx.Message.OrderCode));

        Event(() => InventoryReservationFailed, config => config
            .CorrelateBy<string>(saga => saga.OrderCode, ctx => ctx.Message.OrderCode));

        Event(() => PaymentProcessed, config => config
            .CorrelateBy<string>(saga => saga.OrderCode, ctx => ctx.Message.OrderCode));

        Event(() => PaymentFailed, config => config
            .CorrelateBy<string>(saga => saga.OrderCode, ctx => ctx.Message.OrderCode));

        Event(() => InventoryReleased, config => config
            .CorrelateBy<string>(saga => saga.OrderCode, ctx => ctx.Message.OrderCode));
    }

    private void ConfigureInitialState()
    {
        Initially(
            When(OrderCreated)
                .Then(ctx =>
                {
                    var saga = ctx.Saga;
                    var message = ctx.Message;

                    saga.OrderCode = message.OrderCode;
                    saga.CustomerId = message.CustomerId;
                    saga.Amount = message.TotalAmount;
                    saga.PaymentMethod = message.PaymentMethod;
                    saga.Items = message.Items;
                    saga.CreatedAt = DateTime.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<InventoryReservationRequested>(new
                {
                    OrderCode = ctx.Saga.OrderCode,
                    Items = ctx.Saga.Items,
                }))
                .TransitionTo(AwaitingInventory));
    }

    private void ConfigureAwaitingInventoryState()
    {
        During(AwaitingInventory,
            When(InventoryReserved)
                .PublishAsync(ctx => ctx.Init<PaymentProcessRequested>(new
                {
                    OrderCode = ctx.Saga.OrderCode,
                    CustomerId = ctx.Saga.CustomerId,
                    Amount = ctx.Saga.Amount,
                    PaymentMethod = ctx.Saga.PaymentMethod,
                }))
                .TransitionTo(AwaitingPayment),

            When(InventoryReservationFailed)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                .PublishAsync(ctx => ctx.Init<OrderFailedEvent>(new
                {
                    OrderCode = ctx.Saga.OrderCode,
                    Reason = ctx.Saga.FailureReason,
                    FailedAt = DateTime.UtcNow,
                }))
                .TransitionTo(Failed)
                .Finalize());
    }

    private void ConfigureAwaitingPaymentState()
    {
        During(AwaitingPayment,
            When(PaymentProcessed)
                .Then(ctx =>
                {
                    ctx.Saga.CompletedAt = DateTime.UtcNow;
                })
                .PublishAsync(ctx =>
                {
                    var completedAt = ctx.Saga.CompletedAt;
                    if (!completedAt.HasValue)
                    {
                        completedAt = DateTime.UtcNow;
                    }

                    return ctx.Init<OrderCompletedEvent>(new
                    {
                        OrderCode = ctx.Saga.OrderCode,
                        CompletedAt = completedAt.Value,
                    });
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(PaymentFailed)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                .PublishAsync(ctx => ctx.Init<InventoryReleaseRequested>(new
                {
                    OrderCode = ctx.Saga.OrderCode,
                    Items = ctx.Saga.Items,
                }))
                .TransitionTo(Compensating));
    }

    private void ConfigureCompensatingState()
    {
        During(Compensating,
            When(InventoryReleased)
                .PublishAsync(ctx =>
                {
                    var reason = ctx.Saga.FailureReason;
                    if (string.IsNullOrEmpty(reason))
                    {
                        reason = "Payment failed";
                    }

                    return ctx.Init<OrderFailedEvent>(new
                    {
                        OrderCode = ctx.Saga.OrderCode,
                        Reason = reason,
                        FailedAt = DateTime.UtcNow,
                    });
                })
                .TransitionTo(Failed)
                .Finalize());
    }
}
