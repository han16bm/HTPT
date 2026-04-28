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
public class AuthController : BaseApiController
{
    private readonly IAuthService _service;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService service, ILogger<AuthController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // POST /api/auth/auth/dang-nhap
    [HttpPost("dang-nhap")]
    public async Task<ApiResponse<TokenDto>> DangNhap([FromBody] DangNhapRequest request, CancellationToken ct)
    {
        var result = await _service.DangNhapAsync(request, ct);
        return ApiResponse.Ok(result, "Đăng nhập thành công");
    }

    // POST /api/auth/auth/dang-ky  (Customer only)
    [HttpPost("dang-ky")]
    public async Task<ApiResponse<TokenDto>> DangKy([FromBody] DangKyRequest request, CancellationToken ct)
    {
        var result = await _service.DangKyAsync(request, ct);
        return ApiResponse.Ok(result, "Đăng ký thành công");
    }

    // POST /api/auth/auth/lam-moi-token
    [HttpPost("lam-moi-token")]
    public async Task<ApiResponse<TokenDto>> LamMoiToken([FromBody] LamMoiTokenRequest request, CancellationToken ct)
    {
        var result = await _service.LamMoiTokenAsync(request.RefreshToken, ct);
        return ApiResponse.Ok(result, "Làm mới token thành công");
    }

    // POST /api/auth/auth/dang-xuat
    [HttpPost("dang-xuat")]
    public async Task<ApiResponse> DangXuat(CancellationToken ct)
    {
        // Logout vẫn trả OK kể cả khi thiếu/hỏng header — client cần clear token.
        var userId = GetUserIdOrNull();
        if (userId is long uid)
            await _service.DangXuatAsync(uid, ct);

        return ApiResponse.OkEmpty("Đăng xuất thành công");
    }

    // GET /api/auth/auth/thong-tin-nguoi-dung
    [HttpGet("thong-tin-nguoi-dung")]
    public async Task<ApiResponse<NguoiDungDto>> ThongTinNguoiDung(CancellationToken ct)
    {
        var result = await _service.GetThongTinNguoiDungAsync(GetUserId(), ct);
        return ApiResponse.Ok(result);
    }

    [HttpPost("cap-nhat-thong-tin")]
    public async Task<ApiResponse<NguoiDungDto>> CapNhatThongTin([FromBody] CapNhatThongTinRequest request, CancellationToken ct)
    {
        var result = await _service.CapNhatThongTinAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Cập nhật thông tin thành công");
    }

    [HttpPost("doi-mat-khau")]
    public async Task<ApiResponse> DoiMatKhau([FromBody] DoiMatKhauRequest request, CancellationToken ct)
    {
        await _service.DoiMatKhauAsync(GetUserId(), request, ct);
        return ApiResponse.OkEmpty("Đổi mật khẩu thành công");
    }

    // GET /api/auth/auth/healthcheck
    [HttpGet("healthcheck")]
    public Task<ApiResponse<HealthCheckStatus>> Healthcheck()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Auth", DateTime.UtcNow)));
}
