using API.Admin.Interfaces;
using API.Admin.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Admin.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    // GET /api/admin/dashboard/thong-ke-tong-quat
    [HttpGet("thong-ke-tong-quat")]
    public async Task<ApiResponse<DashboardStatsDto>> ThongKeTongQuat(CancellationToken ct)
    {
        var result = await _service.GetStatsAsync(ct);
        return ApiResponse.Ok(result, "Lấy thống kê thành công");
    }

    // GET /api/admin/dashboard/healthcheck
    [HttpGet("healthcheck")]
    public Task<ApiResponse<HealthCheckStatus>> Healthcheck()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Admin", DateTime.UtcNow)));
}
