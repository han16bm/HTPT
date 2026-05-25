using FishShop.Gateway.Middleware;
using FishShop.Gateway.Services;
using netcore.Commons.Correlation;
using netcore.Commons.Observability;
using netcore.Commons.Resilience;
using Serilog;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithProperty("Service", "Gateway")
       .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [cid:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.File("logs/gateway-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [cid:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"));

// Services
builder.Services.AddSingleton<TokenValidator>();
builder.Services.AddCorrelationId();
builder.Services.AddResilientHttpClient();
builder.Services.AddDistributedTracing(configuration, "Gateway");

// CORS
builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowOrigin", p =>
        p.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

var app = builder.Build();

// Pipeline
app.UseCorrelationId();
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        if (http.Items[CorrelationIdConstants.ItemKey] is string cid)
            diag.Set(CorrelationIdConstants.LogPropertyName, cid);
    };
});
app.UseCors("AllowOrigin");

app.UseObservabilityEndpoints();

app.UseMiddleware<PermissionValidationMiddleware>();
app.UseMiddleware<SetApiKeyMiddleware>();

app.MapReverseProxy();

app.Run();
