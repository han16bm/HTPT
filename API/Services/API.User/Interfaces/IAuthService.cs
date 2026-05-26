using API.User.Models.Commands;
using API.User.Models.DTOs;

namespace API.User.Interfaces;

public interface IAuthService
{
    Task<TokenDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<TokenDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<TokenDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(long userId, CancellationToken ct = default);
    Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default);
}
