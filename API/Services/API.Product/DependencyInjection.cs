using API.Product.Extensions;

namespace API.Product;

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
