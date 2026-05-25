using API.User;
using API.User.Models.DTOs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
using netcore.Commons.Attributes;
using netcore.Commons.Correlation;
using netcore.Commons.Extensions;
using netcore.Commons.Observability;
using netcore.Commons.Resilience;
using netcore.Commons.Filters;
using netcore.Commons.Models;
using netcore.Commons.Services;
using netcore.Entities.Extensions;
using Serilog;
using System.Text.Json.Serialization;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithProperty("Service", "API.User")
       .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [cid:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.File("logs/api-user-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [cid:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

// Observability + resilience
services.AddCorrelationId();
services.AddResilientHttpClient();
services.AddDistributedTracing(configuration, "API.User");

// Config
var authConfig = configuration.GetSection("AuthService").Get<AuthConfiguration>()
    ?? throw new InvalidOperationException("AuthService config is missing in appsettings.json");
services.AddSingleton(authConfig);

// Services
services.AddApplicationServices(configuration);
services.AddEntityServices(configuration);

// Controllers
services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseRouteTokenTransformer()));
    options.Conventions.Add(new ApiPrefixRouteConvention("user"));
    options.Filters.Add<TrimStringsActionFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

services.AddUnifiedApiResponse();

// ApiKey
services.Configure<ApiKeyOptions>(configuration.GetSection("ApiKey"));

// Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API.User — Fish Shop", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

var app = builder.Build();

// Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "API.User — Fish Shop";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API.User V1");
    c.RoutePrefix = "swagger";
});

var userAssetsPath = LocalAssetPathResolver.Resolve(
    configuration["LocalAssetStorage:RootPath"],
    app.Environment.ContentRootPath,
    "assets/user");
Directory.CreateDirectory(userAssetsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(userAssetsPath),
    RequestPath = configuration["LocalAssetStorage:RequestPath"] ?? "/api/user/assets"
});
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
