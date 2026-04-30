using API.Product;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using netcore.Commons.Extensions;
using netcore.Commons.Filters;
using netcore.Commons.Models;
using netcore.Entities.Extensions;
using Serilog;
using System.Text.Json.Serialization;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// ── Serilog ──────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs/api-product-.log", rollingInterval: RollingInterval.Day)
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

// ── Services ─────────────────────────────────────
services.AddApplicationServices(configuration);
services.AddEntityServices(configuration);

// ── Controllers ──────────────────────────────────
services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseRouteTokenTransformer()));
    options.Conventions.Add(new ApiPrefixRouteConvention("product"));
    options.Filters.Add<TrimStringsActionFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

services.AddUnifiedApiResponse();

// ── ApiKey ───────────────────────────────────────
services.Configure<ApiKeyOptions>(configuration.GetSection("ApiKey"));

// ── Swagger ──────────────────────────────────────
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API.Product — Fish Shop", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "API.Product — Fish Shop";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API.Product V1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = configuration["LocalAssetStorage:RequestPath"] ?? "/api/product/assets"
});
app.UseSerilogRequestLogging();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
