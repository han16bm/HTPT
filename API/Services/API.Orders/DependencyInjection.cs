using API.Orders.Extensions;

namespace API.Orders;

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
