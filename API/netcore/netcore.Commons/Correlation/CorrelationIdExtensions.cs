using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace netcore.Commons.Correlation;

public static class CorrelationIdExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        services.AddTransient<CorrelationIdHandler>();

        services.ConfigureHttpClientDefaults(builder =>
        {
            builder.AddHttpMessageHandler<CorrelationIdHandler>();
        });

        return services;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static void UseCorrelationIdPublishFilter(this IPublishPipelineConfigurator cfg, IRegistrationContext context)
    {
        cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), context);
    }

    public static void UseCorrelationIdSendFilter(this ISendPipelineConfigurator cfg, IRegistrationContext context)
    {
        cfg.UseSendFilter(typeof(CorrelationIdPublishFilter<>), context);
    }

    public static void UseCorrelationIdConsumeFilter(this IConsumePipeConfigurator cfg, IRegistrationContext context)
    {
        cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);
    }
}
