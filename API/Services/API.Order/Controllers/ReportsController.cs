using API.Order.Interfaces;
using API.Order.Models.DTOs;
using API.Order.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Order.Controllers;

[Audit]
[ApiKey]
[RequireAdmin]
[Route("reports")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    // GET /api/order/reports/revenue
    [HttpGet("revenue")]
    public async Task<ApiResponse<List<RevenueReportRowDto>>> GetRevenue([FromQuery] ReportQuery query, CancellationToken ct)
    {
        var result = await _service.GetRevenueChartAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/order/reports/top-products
    [HttpGet("top-products")]
    public async Task<ApiResponse<List<TopProductReportRowDto>>> GetTopProducts(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetTopProductReportAsync(limit, fromDate, toDate, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/order/reports/order-summary
    [HttpGet("order-summary")]
    public async Task<ApiResponse<OrderSummaryReportDto>> GetOrderSummary(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetOrderSummaryAsync(fromDate, toDate, ct);
        return ApiResponse.Ok(result);
    }
}
