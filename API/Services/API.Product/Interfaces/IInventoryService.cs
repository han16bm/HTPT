using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using netcore.Commons.Models;

namespace API.Product.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryTransactionDto>> GetTransactionsAsync(InventoryQuery query, CancellationToken ct = default);
    Task ImportStockAsync(StockImportRequest request, CancellationToken ct = default);
    Task<List<LowStockProductDto>> GetLowStockAsync(int threshold = 10, CancellationToken ct = default);
    Task ExportStockAsync(string orderCode, List<netcore.Commons.Messages.Events.OrderItemEventDto> items, CancellationToken ct = default);
}
