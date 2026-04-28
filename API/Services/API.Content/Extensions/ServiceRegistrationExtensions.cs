using API.Content.Interfaces;
using API.Content.Services;
using netcore.Commons.Extensions;
using netcore.Commons.Services;

namespace API.Content.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient(nameof(CloudinaryObjectStorageService), client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddHttpContextAccessor();
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddObjectStorage(configuration);

        // Content services
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IContactService, ContactService>();

        // CORS
        services.AddCorsConfiguration();

        return services;
    }
}
