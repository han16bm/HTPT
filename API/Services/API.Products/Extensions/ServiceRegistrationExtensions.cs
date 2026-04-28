using API.Products.Interfaces;
using API.Products.Services;
using netcore.Commons.Extensions;
using netcore.Commons.Services;

namespace API.Products.Extensions;

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

        // Đăng ký services theo interface
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IInventoryService, InventoryService>();

        // CORS
        services.AddCorsConfiguration();

        return services;
    }
}
