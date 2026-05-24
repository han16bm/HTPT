using API.User;
using API.User.Models.DTOs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
using netcore.Commons.Attributes;
using netcore.Commons.Extensions;
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

// ── Serilog ──────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs/api-user-.log", rollingInterval: RollingInterval.Day)
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

// ── Config ───────────────────────────────────────
var authConfig = configuration.GetSection("AuthService").Get<AuthConfiguration>()
    ?? throw new InvalidOperationException("AuthService config is missing in appsettings.json");
services.AddSingleton(authConfig);

// ── Services ─────────────────────────────────────
services.AddApplicationServices(configuration);
services.AddEntityServices(configuration);

// ── Controllers ──────────────────────────────────
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

// ── ApiKey ───────────────────────────────────────
services.Configure<ApiKeyOptions>(configuration.GetSection("ApiKey"));

// ── Swagger ──────────────────────────────────────
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

// ── Middleware Pipeline ───────────────────────────
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
app.UseSerilogRequestLogging();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
