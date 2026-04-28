using API.Auth.Models.Commands;
using API.Auth.Models.DTOs;

namespace API.Auth.Interfaces;

public interface IAuthService
{
    Task<TokenDto> DangNhapAsync(DangNhapRequest request, CancellationToken ct = default);
    Task<TokenDto> DangKyAsync(DangKyRequest request, CancellationToken ct = default);
    Task<TokenDto> LamMoiTokenAsync(string refreshToken, CancellationToken ct = default);
    Task DangXuatAsync(long userId, CancellationToken ct = default);
    Task<NguoiDungDto> GetThongTinNguoiDungAsync(long userId, CancellationToken ct = default);
    Task<NguoiDungDto> CapNhatThongTinAsync(long userId, CapNhatThongTinRequest request, CancellationToken ct = default);
    Task DoiMatKhauAsync(long userId, DoiMatKhauRequest request, CancellationToken ct = default);
    Task<PermissionValidationResponse> KiemTraQuyenAsync(PermissionValidationRequest request, CancellationToken ct = default);
}
