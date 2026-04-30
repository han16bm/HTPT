using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using netcore.Commons.Interfaces;
using netcore.Commons.Models;
using netcore.Commons.Services;

namespace netcore.Commons.Extensions;

public static class ObjectStorageExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalAssetStorageOptions>(configuration.GetSection("LocalAssetStorage"));
        services.AddScoped<IObjectStorageService, LocalAssetStorageService>();
        return services;
    }
}
