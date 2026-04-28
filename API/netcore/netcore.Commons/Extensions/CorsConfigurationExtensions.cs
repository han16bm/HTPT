using Microsoft.Extensions.DependencyInjection;

namespace netcore.Commons.Extensions;

/// <summary>
/// Cấu hình CORS cho mỗi API service.
/// Cho phép Gateway và các FE domain (dev + prod).
/// </summary>
public static class CorsConfigurationExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowOrigin", builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost:5173",   // FE-Customer dev
                        "http://localhost:5174",   // FE-Admin dev
                        "http://localhost:8080",   // Gateway
                        "http://localhost:3000"    // Thêm nếu cần
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            // Policy rộng hơn cho dev/test
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
