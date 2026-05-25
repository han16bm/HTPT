using API.Content;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
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

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithProperty("Service", "API.Content")
       .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [cid:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.File("logs/api-content-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [cid:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

services.AddCorrelationId();
services.AddResilientHttpClient();
services.AddDistributedTracing(configuration, "API.Content");

services.AddApplicationServices(configuration);
services.AddEntityServices(configuration);

services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseRouteTokenTransformer()));
    options.Conventions.Add(new ApiPrefixRouteConvention("content"));
    options.Filters.Add<TrimStringsActionFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

services.AddUnifiedApiResponse();

services.Configure<ApiKeyOptions>(configuration.GetSection("ApiKey"));
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "API.Content — Fish Shop", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "API.Content — Fish Shop";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API.Content V1");
    c.RoutePrefix = "swagger";
});

var contentAssetsPath = LocalAssetPathResolver.Resolve(
    configuration["LocalAssetStorage:RootPath"],
    app.Environment.ContentRootPath,
    "assets/content");
Directory.CreateDirectory(contentAssetsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(contentAssetsPath),
    RequestPath = configuration["LocalAssetStorage:RequestPath"] ?? "/api/content/assets"
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
