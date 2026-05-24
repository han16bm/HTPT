using API.Content;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
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

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs/api-content-.log", rollingInterval: RollingInterval.Day)
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

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
app.UseSerilogRequestLogging();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
