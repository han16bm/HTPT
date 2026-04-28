using API.Content.Extensions;

namespace API.Content;

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
