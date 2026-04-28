using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;

namespace netcore.Commons.Filters;

/// <summary>
/// Bắt tất cả exception chưa được xử lý và trả về <see cref="ApiResponse{T}"/> chuẩn.
/// Đăng ký trong <c>Program.cs</c> qua <c>options.Filters.Add&lt;GlobalExceptionFilter&gt;()</c>.
///
/// <para>Convention:</para>
/// <list type="bullet">
///   <item><see cref="MessageException"/> (4xx business logic) → log <c>Warning</c>, không stack trace.</item>
///   <item>Các exception khác → log <c>Error</c> kèm stack trace.</item>
///   <item>Log luôn enrich Path/Method/TraceId/UserId từ <see cref="HttpContext"/>.</item>
/// </list>
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        int statusCode;
        string message;

        switch (context.Exception)
        {
            case MessageException mex:
                statusCode = mex.StatusCode;
                message = mex.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = 401;
                message = "Không có quyền truy cập";
                break;

            case ArgumentException aex:
                statusCode = 400;
                message = aex.Message;
                break;

            default:
                statusCode = 500;
                message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
                break;
        }

        // Enrich log với request context — giúp trace từ log ngược về request cụ thể.
        var http = context.HttpContext;
        var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = http.TraceIdentifier,
            ["Path"] = http.Request.Path.Value,
            ["Method"] = http.Request.Method,
            ["UserId"] = userId,
            ["StatusCode"] = statusCode
        });

        if (statusCode >= 500)
        {
            // 5xx: lỗi hệ thống thật sự — log Error kèm stack trace.
            _logger.LogError(context.Exception, "Unhandled 5xx exception: {Message}", context.Exception.Message);
        }
        else
        {
            // 4xx: lỗi nghiệp vụ/validation — log Warning, không cần stack trace tránh spam.
            _logger.LogWarning("{StatusCode} {ExceptionType}: {Message}",
                statusCode,
                context.Exception.GetType().Name,
                context.Exception.Message);
        }

        context.Result = new ObjectResult(ApiResponse<object>.Fail(message))
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
