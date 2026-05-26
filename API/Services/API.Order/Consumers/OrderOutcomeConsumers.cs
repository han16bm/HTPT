using MassTransit;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Messages.Events;
using netcore.Entities.Interfaces;

namespace API.Order.Consumers;

public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILogger<OrderCompletedConsumer> _logger;
    private readonly IUnitOfWork _uow;

    public OrderCompletedConsumer(ILogger<OrderCompletedConsumer> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderCode == message.OrderCode, context.CancellationToken);

        if (order == null)
        {
            return;
        }

        var latestPayment = await _uow.Payments.Query()
            .Where(p => p.OrderId == order.Id)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (string.Equals(order.OrderSource, "POS", StringComparison.OrdinalIgnoreCase)
            || string.Equals(order.OrderStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            order.OrderStatus = "COMPLETED";
            order.PaymentStatus = "PAID";
            order.DeliveredAt ??= message.CompletedAt;
        }
        else
        {
            order.OrderStatus = "CONFIRMED";
            if (latestPayment is not null)
            {
                order.PaymentStatus = latestPayment.Status;
            }
        }

        order.ConfirmedAt ??= message.CompletedAt;
        order.UpdatedAt = DateTime.UtcNow;

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Saga completed: {OrderCode} -> {Status}", message.OrderCode, order.OrderStatus);
    }
}

public class OrderFailedConsumer : IConsumer<OrderFailedEvent>
{
    private readonly ILogger<OrderFailedConsumer> _logger;
    private readonly IUnitOfWork _uow;

    public OrderFailedConsumer(ILogger<OrderFailedConsumer> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        var message = context.Message;

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderCode == message.OrderCode, context.CancellationToken);

        if (order == null)
        {
            return;
        }

        order.OrderStatus = "CANCELLED";
        order.CancelledAt = message.FailedAt;

        if (string.IsNullOrEmpty(order.Note))
        {
            order.Note = message.Reason;
        }
        else
        {
            order.Note = order.Note + " | Saga: " + message.Reason;
        }

        order.UpdatedAt = DateTime.UtcNow;

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogWarning("Saga failed: {OrderCode} -> CANCELLED ({Reason})",
            message.OrderCode, message.Reason);
    }
}
