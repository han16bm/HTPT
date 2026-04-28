using API.Auth.Extensions;
using API.Auth.Models.DTOs;

namespace API.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDependencyInjection(configuration);
        return services;
    }
}
