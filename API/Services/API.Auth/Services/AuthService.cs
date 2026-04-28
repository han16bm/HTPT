using API.Auth.Constants;
using API.Auth.Interfaces;
using API.Auth.Models.Commands;
using API.Auth.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IPasswordService _pwd;
    private readonly AuthConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        IJwtService jwt,
        IPasswordService pwd,
        AuthConfiguration config,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _jwt = jwt;
        _pwd = pwd;
        _config = config;
        _logger = logger;
    }

    public async Task<TokenDto> DangNhapAsync(DangNhapRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username, ct);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found {Username}", request.Username);
            throw new MessageException("Tên đăng nhập hoặc mật khẩu không đúng");
        }

        if (user.Status == false)
        {
            _logger.LogWarning("Login failed: User status is false {Username}", request.Username);
            throw new MessageException("Tài khoản đã bị khóa. Vui lòng liên hệ hỗ trợ.");
        }

        if (!_pwd.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Password hash mismatch for {Username}", request.Username);
            throw new MessageException("Tên đăng nhập hoặc mật khẩu không đúng");
        }

        var profile = await GetCustomerProfileAsync(user, ct);
        var customerId = profile?.Id;
        var accessToken = _jwt.GenerateAccessToken(user, user.Role!.Code, customerId);
        var refreshToken = _jwt.GenerateRefreshToken(user);

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _config.AccessTokenExpiryMinutes * 60,
            User = MapToDto(user, profile, customerId),
        };
    }

    public async Task<TokenDto> DangKyAsync(DangKyRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.AnyAsync(u => u.Username == request.Username, ct))
            throw new MessageException("Tên đăng nhập đã tồn tại");

        if (request.Email is not null && await _uow.Users.AnyAsync(u => u.Email == request.Email, ct))
            throw new MessageException("Email đã được sử dụng");

        if (request.Phone is not null && await _uow.Users.AnyAsync(u => u.Phone == request.Phone, ct))
            throw new MessageException("Số điện thoại đã được sử dụng");

        var customerRole = await _uow.Roles.FirstOrDefaultAsync(r => r.Code == AuthConstants.RoleCustomer, ct)
            ?? throw new MessageException("Lỗi cấu hình hệ thống: không tìm thấy role CUSTOMER");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _pwd.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            RoleId = customerRole.Id,
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        var profile = new CustomerProfile
        {
            UserId = user.Id,
            CustomerCode = $"KH{DateTime.UtcNow:yyyyMMddHHmmss}",
            FullName = request.FullName,
            Phone = request.Phone ?? string.Empty,
            Email = request.Email,
        };

        await _uow.CustomerProfiles.AddAsync(profile, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("New customer registered: {Username}", user.Username);

        user.Role = customerRole;
        var accessToken = _jwt.GenerateAccessToken(user, customerRole.Code, profile.Id);
        var refreshToken = _jwt.GenerateRefreshToken(user);

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _config.AccessTokenExpiryMinutes * 60,
            User = MapToDto(user, profile, profile.Id),
        };
    }

    public async Task<TokenDto> LamMoiTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var userId = _jwt.GetUserIdFromExpiredToken(refreshToken)
            ?? throw new UnauthorizedException("Refresh token không hợp lệ");

        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == (decimal)userId, ct)
            ?? throw new UnauthorizedException("Người dùng không tồn tại");

        if (user.Status == false)
            throw new UnauthorizedException("Tài khoản đã bị khóa");

        var profile = await GetCustomerProfileAsync(user, ct);
        var customerId = profile?.Id;
        var newAccessToken = _jwt.GenerateAccessToken(user, user.Role!.Code, customerId);
        var newRefreshToken = _jwt.GenerateRefreshToken(user);

        return new TokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _config.AccessTokenExpiryMinutes * 60,
            User = MapToDto(user, profile, customerId),
        };
    }

    public async Task DangXuatAsync(long userId, CancellationToken ct = default)
    {
        _logger.LogInformation("User {UserId} logged out", userId);
        await Task.CompletedTask;
    }

    public async Task<NguoiDungDto> GetThongTinNguoiDungAsync(long userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == (decimal)userId, ct)
            ?? throw new NotFoundException("Người dùng", userId);

        var profile = await GetCustomerProfileAsync(user, ct);
        return MapToDto(user, profile, profile?.Id);
    }

    public async Task<NguoiDungDto> CapNhatThongTinAsync(long userId, CapNhatThongTinRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == (decimal)userId, ct)
            ?? throw new NotFoundException("Người dùng", userId);

        var normalizedFullName = request.FullName.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var normalizedGender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim().ToUpperInvariant();

        if (normalizedEmail is not null && await _uow.Users.Query().AnyAsync(u => u.Id != user.Id && u.Email == normalizedEmail, ct))
            throw new MessageException("Email đã được sử dụng");

        if (normalizedPhone is not null && await _uow.Users.Query().AnyAsync(u => u.Id != user.Id && u.Phone == normalizedPhone, ct))
            throw new MessageException("Số điện thoại đã được sử dụng");

        user.FullName = normalizedFullName;
        user.Email = normalizedEmail;
        user.Phone = normalizedPhone;
        user.UpdatedAt = DateTime.UtcNow;

        var profile = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        if (profile is not null)
        {
            if (normalizedEmail is not null && await _uow.CustomerProfiles.Query().AnyAsync(p => p.Id != profile.Id && p.Email == normalizedEmail, ct))
                throw new MessageException("Email đã được sử dụng");

            if (normalizedPhone is not null && await _uow.CustomerProfiles.Query().AnyAsync(p => p.Id != profile.Id && p.Phone == normalizedPhone, ct))
                throw new MessageException("Số điện thoại đã được sử dụng");

            profile.FullName = normalizedFullName;
            profile.Email = normalizedEmail;
            profile.Phone = normalizedPhone ?? string.Empty;
            profile.DateOfBirth = request.DateOfBirth;
            profile.Gender = normalizedGender;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return MapToDto(user, profile, profile?.Id);
    }

    public async Task DoiMatKhauAsync(long userId, DoiMatKhauRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Id == (decimal)userId, ct)
            ?? throw new NotFoundException("Người dùng", userId);

        if (!_pwd.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new MessageException("Mật khẩu hiện tại không đúng");

        if (request.CurrentPassword == request.NewPassword)
            throw new MessageException("Mật khẩu mới phải khác mật khẩu hiện tại");

        user.PasswordHash = _pwd.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PermissionValidationResponse> KiemTraQuyenAsync(
        PermissionValidationRequest request, CancellationToken ct = default)
    {
        var principal = _jwt.ValidateToken(request.Token);
        if (principal is null)
            return new PermissionValidationResponse { IsValid = false, Message = "Token không hợp lệ" };

        var userIdStr = principal.FindFirst(AuthConstants.ClaimUserId)?.Value;
        if (!long.TryParse(userIdStr, out var userIdLong))
            return new PermissionValidationResponse { IsValid = false, Message = "Token không hợp lệ" };

        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == (decimal)userIdLong, ct);

        if (user is null || user.Status == false)
            return new PermissionValidationResponse { IsValid = false, Message = "Tài khoản không hợp lệ" };

        return new PermissionValidationResponse
        {
            IsValid = true,
            UserId = (long)user.Id,
            Username = user.Username,
            RoleCode = user.Role?.Code ?? string.Empty,
            Permissions = user.IsAdmin == true || user.Role?.Code == AuthConstants.RoleAdmin
                ? ["*"]
                : [],
        };
    }

    private async Task<CustomerProfile?> GetCustomerProfileAsync(User user, CancellationToken ct)
    {
        if (user.Role?.Code != AuthConstants.RoleCustomer)
            return null;

        return await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
    }

    private static NguoiDungDto MapToDto(User user, CustomerProfile? profile, decimal? customerId) => new()
    {
        Id = (long)user.Id,
        Username = user.Username,
        CustomerCode = profile?.CustomerCode,
        FullName = profile?.FullName ?? user.FullName,
        Email = profile?.Email ?? user.Email,
        Phone = profile?.Phone ?? user.Phone,
        DateOfBirth = profile?.DateOfBirth,
        Gender = profile?.Gender,
        AvatarUrl = user.AvatarUrl,
        RoleCode = user.Role?.Code ?? string.Empty,
        RoleName = user.Role?.Name ?? string.Empty,
        IsAdmin = user.IsAdmin == true || user.Role?.Code == AuthConstants.RoleAdmin,
        CustomerId = customerId.HasValue ? (long)customerId.Value : null,
    };
}
