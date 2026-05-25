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
[Route("sales")]
public class SalesController : BaseApiController
{
    private readonly ISalesService _service;

    public SalesController(ISalesService service)
    {
        _service = service;
    }

    // GET /api/order/sales/stats
    [HttpGet("stats")]
    public async Task<ApiResponse<List<RevenueChartDto>>> GetStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;
        var result = await _service.GetSalesStatsAsync(from, to, ct);
        return ApiResponse.Ok(result);
    }
}
