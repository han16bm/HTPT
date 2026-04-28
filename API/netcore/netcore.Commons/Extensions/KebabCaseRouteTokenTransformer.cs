using Microsoft.AspNetCore.Routing;

namespace netcore.Commons.Extensions;

/// <summary>
/// Chuyển controller/action name sang kebab-case.
/// ProductsController → /products | TimKiem → /tim-kiem
/// </summary>
public class KebabCaseRouteTokenTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null) return null;

        var str = value.ToString()!;
        // Chèn dấu gạch ngang trước mỗi chữ hoa (trừ ký tự đầu)
        return System.Text.RegularExpressions.Regex
            .Replace(str, "([a-z])([A-Z])", "$1-$2")
            .ToLowerInvariant();
    }
}
