using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;

namespace netcore.Commons.Filters;

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
            _logger.LogError(context.Exception, "Unhandled 5xx exception: {Message}", context.Exception.Message);
        else
            _logger.LogWarning("{StatusCode} {ExceptionType}: {Message}",
                statusCode, context.Exception.GetType().Name, context.Exception.Message);

        context.Result = new ObjectResult(ApiResponse<object>.Fail(message))
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;
    }
}
