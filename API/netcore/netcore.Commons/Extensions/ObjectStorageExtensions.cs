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
        services.Configure<CloudinaryOptions>(options =>
        {
            configuration.GetSection("Cloudinary").Bind(options);
            if (string.IsNullOrWhiteSpace(options.CloudinaryUrl))
            {
                options.CloudinaryUrl = configuration["CLOUDINARY_URL"] ?? string.Empty;
            }
        });
        services.AddScoped<IObjectStorageService, CloudinaryObjectStorageService>();
        return services;
    }
}
