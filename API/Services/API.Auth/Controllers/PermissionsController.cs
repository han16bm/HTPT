using API.Auth.Interfaces;
using API.Auth.Models.Commands;
using API.Auth.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Auth.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class PermissionsController : BaseApiController
{
    private readonly IAuthService _service;

    public PermissionsController(IAuthService service)
    {
        _service = service;
    }

    // POST /api/auth/permissions/check
    [HttpPost("check")]
    public async Task<ApiResponse<PermissionValidationResponse>> Check([FromBody] PermissionValidationRequest request, CancellationToken ct)
    {
        var result = await _service.KiemTraQuyenAsync(request, ct);
        return ApiResponse.Ok(result);
    }
}
