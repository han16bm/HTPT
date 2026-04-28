using API.Products.Interfaces;
using API.Products.Models.Commands;
using API.Products.Models.DTOs;
using API.Products.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Products.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _service;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService service, ILogger<InventoryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/products/inventory/lich-su-giao-dich
    [HttpGet("lich-su-giao-dich")]
    public async Task<ApiResponse<PagedResult<InventoryTransactionDto>>> LichSuGiaoDich([FromQuery] InventoryQuery query, CancellationToken ct)
    {
        var result = await _service.GetLichSuAsync(query, ct);
        return ApiResponse.Ok(result, "Lấy lịch sử giao dịch thành công");
    }

    // POST /api/products/inventory/nhap-hang
    [HttpPost("nhap-hang")]
    public async Task<ApiResponse> NhapHang([FromBody] NhapHangRequest request, CancellationToken ct)
    {
        await _service.NhapHangAsync(request, ct);
        return ApiResponse.OkEmpty("Nhập hàng thành công");
    }

    // GET /api/products/inventory/san-pham-sap-het-hang?threshold=10
    [HttpGet("san-pham-sap-het-hang")]
    public async Task<ApiResponse<List<LowStockProductDto>>> SanPhamSapHetHang([FromQuery] int threshold = 10, CancellationToken ct = default)
    {
        var result = await _service.GetSapHetHangAsync(threshold, ct);
        return ApiResponse.Ok(result, $"Danh sách sản phẩm sắp hết hàng (≤{threshold})");
    }
}
