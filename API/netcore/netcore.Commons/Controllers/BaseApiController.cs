using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Exceptions;

namespace netcore.Commons.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected const string UserIdHeader = "X-User-Id";
    protected const string UserRoleHeader = "X-User-Role";
    protected const string CustomerIdHeader = "X-Customer-Id";

    protected long GetUserId()
    {
        var header = Request.Headers[UserIdHeader].FirstOrDefault();
        if (header == null)
        {
            throw new UnauthorizedException("Yêu cầu đăng nhập");
        }

        if (!long.TryParse(header, out var id))
        {
            throw new UnauthorizedException("Token không hợp lệ");
        }

        return id;
    }

    protected long? GetUserIdOrNull()
    {
        var header = Request.Headers[UserIdHeader].FirstOrDefault();
        if (header == null)
        {
            return null;
        }

        if (long.TryParse(header, out var id))
        {
            return id;
        }

        return null;
    }

    protected string? GetUserRole()
    {
        return Request.Headers[UserRoleHeader].FirstOrDefault();
    }

    protected long? GetCustomerId()
    {
        var header = Request.Headers[CustomerIdHeader].FirstOrDefault();
        if (header == null)
        {
            return null;
        }

        if (long.TryParse(header, out var id))
        {
            return id;
        }

        return null;
    }
}
