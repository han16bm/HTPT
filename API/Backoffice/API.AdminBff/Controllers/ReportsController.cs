using API.Admin.Interfaces;
using API.Admin.Models.DTOs;
using API.Admin.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Admin.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    // GET /api/admin/reports/doanh-thu
    [HttpGet("doanh-thu")]
    public async Task<ApiResponse<List<RevenueReportRowDto>>> DoanhThu([FromQuery] ReportQuery query, CancellationToken ct)
    {
        var result = await _service.GetRevenueChartAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/admin/reports/san-pham-ban-chay
    [HttpGet("san-pham-ban-chay")]
    public async Task<ApiResponse<List<TopProductReportRowDto>>> SanPhamBanChay(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetTopProductReportAsync(limit, fromDate, toDate, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/admin/reports/tong-hop-don-hang
    [HttpGet("tong-hop-don-hang")]
    public async Task<ApiResponse<OrderSummaryReportDto>> TongHopDonHang(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetOrderSummaryAsync(fromDate, toDate, ct);
        return ApiResponse.Ok(result);
    }
}
