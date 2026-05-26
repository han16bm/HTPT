using API.User.Constants;
using API.User.Interfaces;
using API.User.Models.Commands;
using API.User.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;
using UserEntity = netcore.Entities.Entities.User;

namespace API.User.Services;

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

    public async Task<TokenDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
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

    public async Task<TokenDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.AnyAsync(u => u.Username == request.Username, ct))
            throw new MessageException("Tên đăng nhập đã tồn tại");

        if (request.Email is not null && await _uow.Users.AnyAsync(u => u.Email == request.Email, ct))
            throw new MessageException("Email đã được sử dụng");

        if (request.Phone is not null && await _uow.Users.AnyAsync(u => u.Phone == request.Phone, ct))
            throw new MessageException("Số điện thoại đã được sử dụng");

        var customerRole = await _uow.Roles.FirstOrDefaultAsync(r => r.Code == AuthConstants.RoleCustomer, ct)
            ?? throw new MessageException("Lỗi cấu hình hệ thống: không tìm thấy role CUSTOMER");

        var user = new UserEntity
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

    public async Task<TokenDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
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

    public async Task LogoutAsync(long userId, CancellationToken ct = default)
    {
        _logger.LogInformation("User {UserId} logged out", userId);
        await Task.CompletedTask;
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == (decimal)userId, ct)
            ?? throw new NotFoundException("Người dùng", userId);

        var profile = await GetCustomerProfileAsync(user, ct);
        return MapToDto(user, profile, profile?.Id);
    }

    public async Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == (decimal)userId, ct)
            ?? throw new NotFoundException("Người dùng", userId);

        var normalizedFullName = request.FullName.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var normalizedAddressLine = NormalizeOptional(request.AddressLine);
        var normalizedWard = NormalizeOptional(request.Ward);
        var normalizedDistrict = NormalizeOptional(request.District);
        var normalizedProvince = NormalizeOptional(request.Province);
        var normalizedAddress = BuildAddress(
            request.Address,
            normalizedAddressLine,
            normalizedWard,
            normalizedDistrict,
            normalizedProvince);
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
            profile.Address = normalizedAddress;
            profile.AddressLine = normalizedAddressLine;
            profile.Ward = normalizedWard;
            profile.District = normalizedDistrict;
            profile.Province = normalizedProvince;
            profile.DateOfBirth = request.DateOfBirth;
            profile.Gender = normalizedGender;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return MapToDto(user, profile, profile?.Id);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default)
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

    private async Task<CustomerProfile?> GetCustomerProfileAsync(UserEntity user, CancellationToken ct)
    {
        if (user.Role?.Code != AuthConstants.RoleCustomer)
            return null;

        return await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
    }

    private static UserDto MapToDto(UserEntity user, CustomerProfile? profile, decimal? customerId)
    {
        var isAdmin = false;
        if (user.IsAdmin == true)
        {
            isAdmin = true;
        }
        else if (user.Role != null && user.Role.Code == AuthConstants.RoleAdmin)
        {
            isAdmin = true;
        }

        long? mappedCustomerId = null;
        if (customerId.HasValue)
        {
            mappedCustomerId = (long)customerId.Value;
        }

        return new UserDto
        {
            Id = (long)user.Id,
            Username = user.Username,
            CustomerCode = profile?.CustomerCode,
            FullName = profile?.FullName ?? user.FullName,
            Email = profile?.Email ?? user.Email,
            Phone = profile?.Phone ?? user.Phone,
            Address = BuildAddress(profile?.Address, profile?.AddressLine, profile?.Ward, profile?.District, profile?.Province),
            AddressLine = profile?.AddressLine,
            Ward = profile?.Ward,
            District = profile?.District,
            Province = profile?.Province,
            DateOfBirth = profile?.DateOfBirth,
            Gender = profile?.Gender,
            AvatarUrl = user.AvatarUrl,
            RoleCode = user.Role?.Code ?? string.Empty,
            RoleName = user.Role?.Name ?? string.Empty,
            IsAdmin = isAdmin,
            CustomerId = mappedCustomerId,
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return value.Trim();
    }

    private static string? BuildAddress(string? fallback, params string?[] parts)
    {
        var nonEmptyParts = new List<string>();
        foreach (var part in parts)
        {
            if (!string.IsNullOrWhiteSpace(part))
            {
                nonEmptyParts.Add(part.Trim());
            }
        }

        if (nonEmptyParts.Count == 0)
        {
            return NormalizeOptional(fallback);
        }

        return string.Join(", ", nonEmptyParts);
    }
}
