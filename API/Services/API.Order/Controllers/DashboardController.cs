using API.Order.Interfaces;
using API.Order.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Order.Controllers;

[Audit]
[ApiKey]
[RequireAdmin]
[Route("dashboard")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    // GET /api/order/dashboard/stats
    [HttpGet("stats")]
    public async Task<ApiResponse<DashboardStatsDto>> GetStats(CancellationToken ct)
    {
        var result = await _service.GetStatsAsync(ct);
        return ApiResponse.Ok(result, "Lấy thống kê thành công");
    }

    // GET /api/order/dashboard/health
    [HttpGet("health")]
    public Task<ApiResponse<HealthCheckStatus>> Health()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Order", DateTime.UtcNow)));
}
