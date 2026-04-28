using API.Content;
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

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs/api-content-.log", rollingInterval: RollingInterval.Day));

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

app.UseSerilogRequestLogging();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
