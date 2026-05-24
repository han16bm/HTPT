using API.User.Interfaces;
using API.User.Services;
using netcore.Commons.Extensions;

namespace API.User.Extensions;

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
        services.AddObjectStorage(configuration);

        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICustomerAdminService, CustomerAdminService>();

        // CORS
        services.AddCorsConfiguration();

        return services;
    }
}
