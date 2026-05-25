using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using netcore.Commons.Models;

namespace netcore.Commons.Extensions;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection AddUnifiedApiResponse(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .SelectMany(kv => kv.Value!.Errors.Select(e =>
                        string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? $"{kv.Key}: {e.Exception?.Message ?? "invalid"}"
                            : $"{kv.Key}: {e.ErrorMessage}"))
                    .ToList();

                var response = ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ", errors);
                return new BadRequestObjectResult(response);
            };
        });
        return services;
    }
}
