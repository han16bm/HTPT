using API.Orders.Interfaces;
using API.Orders.Services;
using netcore.Commons.Extensions;

namespace API.Orders.Extensions;

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

        // Order services
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPromotionService, PromotionService>();

        // CORS
        services.AddCorsConfiguration();

        return services;
    }
}
