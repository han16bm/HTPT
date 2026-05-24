using API.User.Extensions;
using API.User.Models.DTOs;

namespace API.User;

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
