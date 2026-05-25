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
    public static IServiceCollection AddEntityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var logger = sp.GetRequiredService<ILogger<AppDbContext>>();
            logger.LogInformation("SQL Server ConnectionString loaded.");

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
