using API.Product.Interfaces;
using MassTransit;
using netcore.Commons.Messages.Events;

namespace API.Product.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventConsumer> _logger;
    private readonly IInventoryService _inventoryService;

    public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger, IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Nhận được event tạo đơn hàng: {OrderCode} với {ItemCount} sản phẩm", message.OrderCode, message.Items.Count);

        try
        {
            await _inventoryService.ExportStockAsync(message.OrderCode, message.Items, context.CancellationToken);
            _logger.LogInformation("Xử lý tồn kho qua Message Queue cho đơn hàng: {OrderCode}", message.OrderCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý message tạo đơn hàng: {OrderCode}", message.OrderCode);
            throw; // To let MassTransit retry or move to dead-letter queue
        }
    }
}
