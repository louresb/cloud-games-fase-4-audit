using Fiap.CloudGames.Audit.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fiap.CloudGames.Audit.Application.Consumers;

public abstract class AuditConsumerBase<TEvent>(IAuditService audit, ILogger<AuditConsumerBase<TEvent>> logger)
    : IConsumer<TEvent>
    where TEvent : class
{
    protected abstract string SourceService { get; }
    protected virtual string? AggregateIdFor(TEvent message) => null;

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var correlationId = context.CorrelationId?.ToString()
            ?? (context.Headers.TryGetHeader("X-Correlation-Id", out var raw) ? raw?.ToString() : null);

        string? tenantId = null;
        if (context.Headers.TryGetHeader("X-Tenant-Id", out var tenantHeader)
            || context.Headers.TryGetHeader("TenantId", out tenantHeader))
        {
            tenantId = tenantHeader?.ToString();
        }

        tenantId ??= typeof(TEvent).GetProperty("TenantId")?.GetValue(context.Message)?.ToString();

        var eventType = typeof(TEvent).Name;

        try
        {
            await audit.RecordAsync(
                payload: context.Message,
                eventType: eventType,
                sourceService: SourceService,
                aggregateId: AggregateIdFor(context.Message),
                correlationId: correlationId,
                tenantId: tenantId,
                ct: context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to audit {EventType} (correlation={Correlation}, tenant={Tenant})",
                eventType, correlationId, tenantId);
            throw;
        }
    }
}
