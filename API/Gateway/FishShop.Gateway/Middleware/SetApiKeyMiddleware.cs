namespace FishShop.Gateway.Middleware;

/// <summary>
/// Injects the internal API key into every forwarded request
/// so downstream services can verify the request came through the Gateway.
/// </summary>
public class SetApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public SetApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = _config["Gateway:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            // Overwrite or add X-Api-Key header so downstream [ApiKey] attribute passes
            context.Request.Headers["X-Api-Key"] = apiKey;
        }

        await _next(context);
    }
}
