using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace netcore.Commons.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuditAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var loggerFactory = context.HttpContext.RequestServices
            .GetService(typeof(ILoggerFactory)) as ILoggerFactory;
        var logger = loggerFactory?.CreateLogger("Audit");

        var userId = context.HttpContext.Request.Headers["X-User-Id"].FirstOrDefault() ?? "anonymous";
        var userName = context.HttpContext.Request.Headers["X-User-Name"].FirstOrDefault() ?? "anonymous";
        var path = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method;
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        logger?.LogInformation("[AUDIT] {Method} {Path} | User: {UserId}/{UserName} | IP: {IP}",
            method, path, userId, userName, ip);

        var resultContext = await next();

        if (resultContext.Exception != null)
        {
            logger?.LogError(resultContext.Exception,
                "[AUDIT] ERROR {Method} {Path} | User: {UserId}",
                method, path, userId);
        }
    }
}
