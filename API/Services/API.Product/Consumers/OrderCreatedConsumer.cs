using netcore.Commons.Contracts.Events;

namespace API.Product.Consumers;

public sealed class OrderCreatedConsumer
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task ConsumeAsync(OrderCreatedEvent message, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Received order-created event {OrderCode} with {ItemCount} item(s)",
            message.OrderCode,
            message.Items.Count);

        return Task.CompletedTask;
    }
}
