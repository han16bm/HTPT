using MassTransit;
using Microsoft.AspNetCore.Http;

namespace netcore.Commons.Correlation;

public sealed class CorrelationIdPublishFilter<T> : IFilter<PublishContext<T>>, IFilter<SendContext<T>>
    where T : class
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdPublishFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        ApplyHeader(context);
        return next.Send(context);
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        ApplyHeader(context);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlation-id-publish");
    }

    private void ApplyHeader(SendContext context)
    {
        HttpContext? httpContext;
        try
        {
            httpContext = _httpContextAccessor.HttpContext;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (httpContext == null)
        {
            return;
        }

        string? correlationId;
        try
        {
            correlationId = httpContext.Items[CorrelationIdConstants.ItemKey] as string;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        context.Headers.Set(CorrelationIdConstants.MessageHeaderName, correlationId);

        if (!context.CorrelationId.HasValue && Guid.TryParse(correlationId, out var parsedGuid))
        {
            context.CorrelationId = parsedGuid;
        }
    }
}
