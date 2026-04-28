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
public class SalesController : BaseApiController
{
    private readonly ISalesService _service;

    public SalesController(ISalesService service) => _service = service;

    // GET /api/admin/sales/thong-ke
    [HttpGet("thong-ke")]
    public async Task<ApiResponse<List<RevenueChartDto>>> ThongKe(
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
