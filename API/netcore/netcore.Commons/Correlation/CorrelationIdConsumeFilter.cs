using MassTransit;
using LogContext = Serilog.Context.LogContext;

namespace netcore.Commons.Correlation;

public sealed class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.Headers.Get<string>(CorrelationIdConstants.MessageHeaderName);

        if (string.IsNullOrWhiteSpace(correlationId) && context.CorrelationId.HasValue)
        {
            correlationId = context.CorrelationId.Value.ToString("N");
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        using (LogContext.PushProperty(CorrelationIdConstants.LogPropertyName, correlationId))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlation-id-consume");
    }
}
