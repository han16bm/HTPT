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
[Route("auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _service;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService service, ILogger<AuthController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // POST /api/user/auth/login
    [HttpPost("login")]
    public async Task<ApiResponse<TokenDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, ct);
        return ApiResponse.Ok(result, "Đăng nhập thành công");
    }

    // POST /api/user/auth/register
    [HttpPost("register")]
    public async Task<ApiResponse<TokenDto>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(request, ct);
        return ApiResponse.Ok(result, "Đăng ký thành công");
    }

    // POST /api/user/auth/refresh-token
    [HttpPost("refresh-token")]
    public async Task<ApiResponse<TokenDto>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _service.RefreshTokenAsync(request.RefreshToken, ct);
        return ApiResponse.Ok(result, "Làm mới token thành công");
    }

    // POST /api/user/auth/logout
    [HttpPost("logout")]
    public async Task<ApiResponse> Logout(CancellationToken ct)
    {
        // Logout vẫn trả OK kể cả khi thiếu/hỏng header — client cần clear token.
        var userId = GetUserIdOrNull();
        if (userId is long uid)
            await _service.LogoutAsync(uid, ct);

        return ApiResponse.OkEmpty("Đăng xuất thành công");
    }

    // GET /api/user/auth/me
    [HttpGet("me")]
    public async Task<ApiResponse<UserDto>> GetMe(CancellationToken ct)
    {
        var result = await _service.GetCurrentUserAsync(GetUserId(), ct);
        return ApiResponse.Ok(result);
    }

    // PUT /api/user/auth/me
    [HttpPut("me")]
    public async Task<ApiResponse<UserDto>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateProfileAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Cập nhật thông tin thành công");
    }

    // PUT /api/user/auth/password
    [HttpPut("password")]
    public async Task<ApiResponse> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _service.ChangePasswordAsync(GetUserId(), request, ct);
        return ApiResponse.OkEmpty("Đổi mật khẩu thành công");
    }

    // GET /api/user/auth/health
    [HttpGet("health")]
    public Task<ApiResponse<HealthCheckStatus>> Health()
    {
        var status = new HealthCheckStatus("Healthy", "API.User", DateTime.UtcNow);
        return Task.FromResult(ApiResponse.Ok(status));
    }
}
