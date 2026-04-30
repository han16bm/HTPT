using FishShop.Gateway.Services;
using System.Text.Json;

namespace FishShop.Gateway.Middleware;

/// <summary>
/// Validates JWT on every request:
/// - Public paths → pass through directly
/// - Protected paths → validate token, inject X-User-Id + X-User-Role headers
/// </summary>
public class PermissionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly TokenValidator _tokenValidator;
    private readonly ILogger<PermissionValidationMiddleware> _logger;

    public PermissionValidationMiddleware(
        RequestDelegate next,
        IConfiguration config,
        TokenValidator tokenValidator,
        ILogger<PermissionValidationMiddleware> logger)
    {
        _next = next;
        _config = config;
        _tokenValidator = tokenValidator;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // CORS preflight — always pass
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        // Check if this path is public (no auth required)
        var unauthPaths = _config.GetSection("Gateway:UnauthenticatedPaths").Get<List<string>>() ?? [];
        bool isPublic = unauthPaths.Any(pub => IsPublicPath(pub, context.Request.Method, path));

        if (isPublic)
        {
            await _next(context);
            return;
        }

        // Extract bearer token
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorized(context, "Yêu cầu đăng nhập. Thiếu token xác thực.");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var principal = _tokenValidator.ValidateToken(token);

        if (principal is null)
        {
            await WriteUnauthorized(context, "Token không hợp lệ hoặc đã hết hạn.");
            return;
        }

        // Extract claims and inject as headers for downstream services
        var userId = _tokenValidator.GetClaim(principal, "sub")
                  ?? _tokenValidator.GetClaim(principal, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        var roleCode = _tokenValidator.GetClaim(principal, "role")
                    ?? _tokenValidator.GetClaim(principal, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        var customerId = _tokenValidator.GetClaim(principal, "customer_id");

        if (!string.IsNullOrWhiteSpace(userId))
            context.Request.Headers["X-User-Id"] = userId;

        if (!string.IsNullOrWhiteSpace(roleCode))
            context.Request.Headers["X-User-Role"] = roleCode;

        if (!string.IsNullOrWhiteSpace(customerId))
            context.Request.Headers["X-Customer-Id"] = customerId;

        _logger.LogDebug("Request authorized: userId={UserId} role={Role} → {Path}", userId, roleCode, path);

        await _next(context);
    }

    private static Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new
        {
            success = false,
            message,
            data = (object?)null,
            errors = (object?)null,
        });
        return context.Response.WriteAsync(body);
    }

    private static bool IsPublicPath(string rule, string requestMethod, string requestPath)
    {
        var normalizedRule = rule.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRule))
        {
            return false;
        }

        var spaceIndex = normalizedRule.IndexOf(' ');
        if (spaceIndex > 0)
        {
            var method = normalizedRule[..spaceIndex].Trim();
            var path = normalizedRule[(spaceIndex + 1)..].Trim().ToLowerInvariant();

            return requestMethod.Equals(method, StringComparison.OrdinalIgnoreCase)
                && requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase);
        }

        return requestPath.StartsWith(normalizedRule.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
    }
}
