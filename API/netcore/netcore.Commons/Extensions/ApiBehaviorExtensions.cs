using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using netcore.Commons.Models;

namespace netcore.Commons.Extensions;

/// <summary>
/// Cấu hình hành vi chung của ASP.NET Core MVC cho toàn bộ services.
/// </summary>
public static class ApiBehaviorExtensions
{
    /// <summary>
    /// Thay default <c>ProblemDetails</c> (RFC 9110) của model-binding/validation
    /// bằng <see cref="ApiResponse{T}"/>.Fail chuẩn — để mọi response lỗi đều
    /// cùng một shape <c>{ success, message, errors }</c>.
    ///
    /// <para>Gọi trong <c>Program.cs</c> SAU <c>AddControllers()</c>:</para>
    /// <code>services.AddUnifiedApiResponse();</code>
    /// </summary>
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
