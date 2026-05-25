namespace FishShop.Gateway.Middleware;

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
            context.Request.Headers["X-Api-Key"] = apiKey;

        await _next(context);
    }
}
