using API.Order.Consumers;
using API.Order.Interfaces;
using API.Order.Saga;
using API.Order.Services;
using netcore.Commons.Correlation;
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

        // MassTransit (RabbitMQ) — publisher + payment consumer + Order saga
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentProcessConsumer>();
            x.AddConsumer<OrderCompletedConsumer>();
            x.AddConsumer<OrderFailedConsumer>();

            // InMemoryRepository: state mất khi restart. Production cần đổi sang EntityFrameworkRepository.
            x.AddSagaStateMachine<OrderStateMachine, OrderSagaState>()
                .InMemoryRepository();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.UseCorrelationIdPublishFilter(context);
                cfg.UseCorrelationIdSendFilter(context);

                cfg.ReceiveEndpoint("payment-process-queue", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(3)));
                    e.UseCorrelationIdConsumeFilter(context);
                    e.ConfigureConsumer<PaymentProcessConsumer>(context);
                });
                cfg.ReceiveEndpoint("order-completed-queue", e =>
                {
                    e.UseCorrelationIdConsumeFilter(context);
                    e.ConfigureConsumer<OrderCompletedConsumer>(context);
                });
                cfg.ReceiveEndpoint("order-failed-queue", e =>
                {
                    e.UseCorrelationIdConsumeFilter(context);
                    e.ConfigureConsumer<OrderFailedConsumer>(context);
                });
                cfg.ReceiveEndpoint("order-saga", e =>
                {
                    e.UseCorrelationIdConsumeFilter(context);
                    e.ConfigureSaga<OrderSagaState>(context);
                });
            });
        });

        return services;
    }
}
