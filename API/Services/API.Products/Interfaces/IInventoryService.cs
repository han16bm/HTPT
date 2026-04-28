using API.Products.Models.Commands;
using API.Products.Models.DTOs;
using API.Products.Models.Queries;
using netcore.Commons.Models;

namespace API.Products.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryTransactionDto>> GetLichSuAsync(InventoryQuery query, CancellationToken ct = default);
    Task NhapHangAsync(NhapHangRequest request, CancellationToken ct = default);
    Task<List<LowStockProductDto>> GetSapHetHangAsync(int threshold = 10, CancellationToken ct = default);
}
