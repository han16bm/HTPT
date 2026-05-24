using API.User.Interfaces;
using API.User.Models.Commands;
using API.User.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.User.Controllers;

[Audit]
[ApiKey]
[Route("permissions")]
public class PermissionsController : BaseApiController
{
    private readonly IAuthService _service;

    public PermissionsController(IAuthService service)
    {
        _service = service;
    }

    // POST /api/user/permissions/check
    [HttpPost("check")]
    public async Task<ApiResponse<PermissionValidationResponse>> Check([FromBody] PermissionValidationRequest request, CancellationToken ct)
    {
        var result = await _service.ValidatePermissionAsync(request, ct);
        return ApiResponse.Ok(result);
    }
}
