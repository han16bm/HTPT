using API.Product.Consumers;
using API.Product.Interfaces;
using API.Product.Services;
using netcore.Commons.Correlation;
using netcore.Commons.Extensions;
using MassTransit;

namespace API.Product.Extensions;

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

        // Đăng ký services theo interface
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IInventoryService, InventoryService>();
        // MassTransit (RabbitMQ Consumer) - saga inventory steps
        services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryReservationConsumer>();
            x.AddConsumer<InventoryReleaseConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ReceiveEndpoint("inventory-reservation-queue", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.UseCorrelationIdConsumeFilter(context);
                    e.ConfigureConsumer<InventoryReservationConsumer>(context);
                });
                cfg.ReceiveEndpoint("inventory-release-queue", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.UseCorrelationIdConsumeFilter(context);
                    e.ConfigureConsumer<InventoryReleaseConsumer>(context);
                });
            });
        });

        // CORS
        services.AddCorsConfiguration();

        return services;
    }
}
