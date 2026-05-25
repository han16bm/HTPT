using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using netcore.Commons.Correlation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace netcore.Commons.Observability;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddDistributedTracing(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["Otlp:Endpoint"];
        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            otlpEndpoint = "http://jaeger:4317";
        }

        var builder = services.AddOpenTelemetry();

        builder.ConfigureResource(resource => ConfigureResource(resource, serviceName));
        builder.WithTracing(tracing => ConfigureTracing(tracing, otlpEndpoint));
        builder.WithMetrics(metrics => ConfigureMetrics(metrics));

        return services;
    }

    public static IApplicationBuilder UseObservabilityEndpoints(this IApplicationBuilder app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
        return app;
    }

    private static void ConfigureResource(ResourceBuilder resource, string serviceName)
    {
        resource.AddService(
            serviceName: serviceName,
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName);
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, string otlpEndpoint)
    {
        tracing.AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = ShouldTraceRequest;
            options.EnrichWithHttpRequest = EnrichSpanFromHttpRequest;
        });

        tracing.AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        });

        tracing.AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.RecordException = true;
        });

        tracing.AddSource("MassTransit");

        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics)
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddMeter("MassTransit");
        metrics.AddPrometheusExporter();
    }

    private static bool ShouldTraceRequest(Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            return true;
        }

        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Contains("/assets/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.EndsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static void EnrichSpanFromHttpRequest(
        System.Diagnostics.Activity activity,
        Microsoft.AspNetCore.Http.HttpRequest request)
    {
        var correlationId = request.HttpContext.Items[CorrelationIdConstants.ItemKey] as string;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            activity.SetTag("correlation_id", correlationId);
        }

        var userId = request.Headers["X-User-Id"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            activity.SetTag("user.id", userId);
        }
    }
}
