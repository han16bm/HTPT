using API.Product.Interfaces;
using MassTransit;
using netcore.Commons.Messages.Events;

namespace API.Product.Consumers;

public class InventoryReleaseConsumer : IConsumer<InventoryReleaseRequested>
{
    private readonly ILogger<InventoryReleaseConsumer> _logger;
    private readonly IInventoryService _inventoryService;

    public InventoryReleaseConsumer(
        ILogger<InventoryReleaseConsumer> logger,
        IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public async Task Consume(ConsumeContext<InventoryReleaseRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Saga release inventory: {OrderCode}", message.OrderCode);

        await _inventoryService.ReleaseStockAsync(
            message.OrderCode,
            message.Items,
            context.CancellationToken);

        var releasedEvent = new InventoryReleasedEvent
        {
            OrderCode = message.OrderCode,
        };
        await context.Publish(releasedEvent);
    }
}
