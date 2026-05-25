using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using netcore.Commons.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace netcore.Commons.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : Attribute, IAsyncActionFilter
{
    private const string UserRoleHeader = "X-User-Role";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var role = context.HttpContext.Request.Headers[UserRoleHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(role) || !role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(ApiResponse.Fail("Forbidden: Yêu cầu quyền quản trị viên (ADMIN)."))
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}
