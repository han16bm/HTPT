using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using netcore.Entities.Interfaces;
using netcore.Entities.Persistence;
using netcore.Entities.Repositories;

namespace netcore.Entities.Extensions;

public static class EntityServicesExtensions
{
    /// <summary>
    /// Đăng ký DbContext (Oracle) + IUnitOfWork vào DI container.
    /// Gọi trong Program.cs của mỗi API service.
    /// </summary>
    public static IServiceCollection AddEntityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var logger = sp.GetRequiredService<ILogger<AppDbContext>>();
            logger.LogInformation("Oracle ConnectionString loaded.");

            options.UseOracle(connectionString, o =>
            {
                o.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
            });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
