using Microsoft.AspNetCore.Http;

namespace netcore.Commons.Correlation;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CorrelationId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }
            return httpContext.Items[CorrelationIdConstants.ItemKey] as string;
        }
    }
}

public static class CorrelationIdConstants
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";
    public const string LogPropertyName = "CorrelationId";
    public const string MessageHeaderName = "correlation-id";
}
