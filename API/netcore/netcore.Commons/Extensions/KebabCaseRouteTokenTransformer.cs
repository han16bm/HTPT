using Microsoft.AspNetCore.Routing;

namespace netcore.Commons.Extensions;

/// <summary>
/// Chuyển controller/action name sang kebab-case.
/// ProductsController -> /products | SearchProducts -> /search-products
/// </summary>
public class KebabCaseRouteTokenTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null) return null;

        var str = value.ToString()!;
        // Insert a dash before each uppercase transition.
        return System.Text.RegularExpressions.Regex
            .Replace(str, "([a-z])([A-Z])", "$1-$2")
            .ToLowerInvariant();
    }
}
