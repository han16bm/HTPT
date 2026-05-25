using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using netcore.Commons.Models;

namespace netcore.Commons.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<ApiKeyOptions>>().Value;

        if (!context.HttpContext.Request.Headers.TryGetValue(options.HeaderName, out var extractedApiKey)
            || extractedApiKey != options.Key)
        {
            context.Result = new ObjectResult(ApiResponse.Fail("Unauthorized: Invalid API Key"))
            {
                StatusCode = 401
            };
            return;
        }

        await next();
    }
}
