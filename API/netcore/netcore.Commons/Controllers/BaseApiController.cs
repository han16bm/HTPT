using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Exceptions;

namespace netcore.Commons.Controllers;

/// <summary>
/// Base controller cho mọi API service downstream gateway.
/// Gateway đã validate JWT và forward identity qua headers:
///   X-User-Id, X-User-Role, X-Customer-Id.
/// Services không tự đọc JWT — chỉ đọc header.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected const string UserIdHeader = "X-User-Id";
    protected const string UserRoleHeader = "X-User-Role";
    protected const string CustomerIdHeader = "X-Customer-Id";

    /// <summary>
    /// Lấy UserId từ header <c>X-User-Id</c> (gateway đã inject sau khi validate JWT).
    /// Ném <see cref="UnauthorizedException"/> nếu thiếu hoặc không parse được.
    /// Dùng cho endpoint protected.
    /// </summary>
    protected long GetUserId()
    {
        var header = Request.Headers[UserIdHeader].FirstOrDefault()
            ?? throw new UnauthorizedException("Yêu cầu đăng nhập");
        if (!long.TryParse(header, out var id))
            throw new UnauthorizedException("Token không hợp lệ");
        return id;
    }

    /// <summary>
    /// Lấy UserId nếu có, trả về null nếu không đăng nhập.
    /// Dùng cho endpoint optional-auth (ví dụ xem giỏ hàng guest).
    /// </summary>
    protected long? GetUserIdOrNull()
    {
        var header = Request.Headers[UserIdHeader].FirstOrDefault();
        return long.TryParse(header, out var id) ? id : null;
    }

    /// <summary>Role code từ header <c>X-User-Role</c>.</summary>
    protected string? GetUserRole()
        => Request.Headers[UserRoleHeader].FirstOrDefault();

    /// <summary>CustomerId từ header <c>X-Customer-Id</c> (chỉ có khi user là khách hàng).</summary>
    protected long? GetCustomerId()
    {
        var header = Request.Headers[CustomerIdHeader].FirstOrDefault();
        return long.TryParse(header, out var id) ? id : null;
    }
}
