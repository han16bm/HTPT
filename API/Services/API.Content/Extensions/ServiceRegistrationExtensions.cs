using API.Content.Interfaces;
using API.Content.Services;
using netcore.Commons.Extensions;

namespace API.Content.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient();
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
