using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using netcore.Commons.Models;

namespace API.Product.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryTransactionDto>> GetLichSuAsync(InventoryQuery query, CancellationToken ct = default);
    Task NhapHangAsync(NhapHangRequest request, CancellationToken ct = default);
    Task<List<LowStockProductDto>> GetSapHetHangAsync(int threshold = 10, CancellationToken ct = default);
    Task XuatHangAsync(string orderCode, List<netcore.Commons.Messages.Events.OrderItemEventDto> items, CancellationToken ct = default);
}
