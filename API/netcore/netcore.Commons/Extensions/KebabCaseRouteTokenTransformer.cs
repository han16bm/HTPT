using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace netcore.Commons.Extensions;

public class KebabCaseRouteTokenTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var input = value.ToString();
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var withDashes = Regex.Replace(input, "([a-z])([A-Z])", "$1-$2");
        return withDashes.ToLowerInvariant();
    }
}
