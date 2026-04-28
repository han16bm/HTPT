using API.Admin.Interfaces;
using API.Admin.Services;
using netcore.Commons.Extensions;

namespace API.Admin.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddAuthentication();
        services.AddAuthorization();

        // Admin services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<ICustomerAdminService, CustomerAdminService>();
        services.AddScoped<IReportService, ReportService>();

        // CORS
        services.AddCorsConfiguration();

        return services;
    }
}
