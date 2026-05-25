using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace netcore.Commons.Filters;

public class TrimStringsActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;
            TrimStrings(argument, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private static void TrimStrings(object obj, HashSet<object> visited)
    {
        if (obj is null) return;

        var type = obj.GetType();
        if (ShouldSkipType(type) || !visited.Add(obj)) return;

        if (obj is IEnumerable enumerable && obj is not string)
        {
            foreach (var item in enumerable)
            {
                if (item is not null)
                {
                    TrimStrings(item, visited);
                }
            }

            return;
        }

        foreach (var prop in type.GetProperties())
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
            if (ShouldSkipType(prop.PropertyType)) continue;

            if (prop.PropertyType == typeof(string))
            {
                var value = prop.GetValue(obj) as string;
                if (value is not null && prop.CanWrite)
                    prop.SetValue(obj, value.Trim());
            }
            else
            {
                var nested = prop.GetValue(obj);
                if (nested is not null)
                {
                    TrimStrings(nested, visited);
                }
            }
        }
    }

    private static bool ShouldSkipType(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
            return true;

        if (type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            type == typeof(CancellationToken))
            return true;

        if (typeof(IFormFile).IsAssignableFrom(type) ||
            typeof(IFormFileCollection).IsAssignableFrom(type) ||
            typeof(Stream).IsAssignableFrom(type))
            return true;

        return false;
    }
}
