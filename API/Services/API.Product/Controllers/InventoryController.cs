using API.Product.Interfaces;
using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Product.Controllers;

[Audit]
[ApiKey]
[Route("inventory")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    // GET /api/product/inventory/transactions
    [HttpGet("transactions")]
    public async Task<ApiResponse<PagedResult<InventoryTransactionDto>>> GetTransactions([FromQuery] InventoryQuery query, CancellationToken ct)
    {
        var result = await _service.GetLichSuAsync(query, ct);
        return ApiResponse.Ok(result, "Lay lich su giao dich thanh cong");
    }

    // POST /api/product/inventory/imports
    [HttpPost("imports")]
    [RequireAdmin]
    public async Task<ApiResponse> Import([FromBody] NhapHangRequest request, CancellationToken ct)
    {
        await _service.NhapHangAsync(request, ct);
        return ApiResponse.OkEmpty("Nhap hang thanh cong");
    }

    // GET /api/product/inventory/low-stock?threshold=10
    [HttpGet("low-stock")]
    public async Task<ApiResponse<List<LowStockProductDto>>> GetLowStock([FromQuery] int threshold = 10, CancellationToken ct = default)
    {
        var result = await _service.GetSapHetHangAsync(threshold, ct);
        return ApiResponse.Ok(result, $"Danh sach san pham sap het hang (<={threshold})");
    }
}
