using API.Product.Interfaces;
using MassTransit;
using netcore.Commons.Messages.Events;

namespace API.Product.Consumers;

public class InventoryReservationConsumer : IConsumer<InventoryReservationRequested>
{
    private readonly ILogger<InventoryReservationConsumer> _logger;
    private readonly IInventoryService _inventoryService;

    public InventoryReservationConsumer(
        ILogger<InventoryReservationConsumer> logger,
        IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public async Task Consume(ConsumeContext<InventoryReservationRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Saga reserve inventory: {OrderCode} ({Count} items)",
            message.OrderCode, message.Items.Count);

        try
        {
            await _inventoryService.ExportStockAsync(
                message.OrderCode,
                message.Items,
                context.CancellationToken);

            var reservedEvent = new InventoryReservedEvent
            {
                OrderCode = message.OrderCode,
            };
            await context.Publish(reservedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Reserve failed {OrderCode}: {Reason}",
                message.OrderCode, ex.Message);

            var failedEvent = new InventoryReservationFailedEvent
            {
                OrderCode = message.OrderCode,
                Reason = ex.Message,
            };
            await context.Publish(failedEvent);
        }
    }
}
