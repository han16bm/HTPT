using API.Order.Interfaces;
using API.Order.Services;
using netcore.Commons.Extensions;
using MassTransit;

namespace API.Order.Extensions;

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
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IReportService, ReportService>();

        // CORS
        services.AddCorsConfiguration();

        // MassTransit (RabbitMQ Publisher)
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });

        return services;
    }
}
