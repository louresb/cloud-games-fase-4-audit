using System.Text.Json;
using Fiap.CloudGames.Audit.Application.Dtos;
using Fiap.CloudGames.Audit.Domain.Entities;
using Fiap.CloudGames.Audit.Domain.Repositories;
using Fiap.CloudGames.Audit.Domain.Tenants;
using Microsoft.Extensions.Logging;

namespace Fiap.CloudGames.Audit.Application.Services;

public sealed class AuditService(IAuditRepository repository, ILogger<AuditService> logger) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public async Task RecordAsync<TEvent>(
        TEvent payload,
        string eventType,
        string sourceService,
        string? aggregateId,
        string? correlationId,
        string? tenantId,
        CancellationToken ct = default) where TEvent : class
    {
        var normalizedTenant = Tenants.Normalize(tenantId);
        var correlation = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString() : correlationId!;
        var payloadJson = JsonSerializer.Serialize(payload, JsonOpts);

        var entry = AuditEntry.Create(
            tenantId: normalizedTenant,
            eventType: eventType,
            sourceService: sourceService,
            correlationId: correlation,
            aggregateId: aggregateId,
            payloadJson: payloadJson);

        await repository.AddAsync(entry, ct);

        logger.LogInformation(
            "Audit recorded {EventType} (tenant={Tenant}, correlation={Correlation}, source={Source})",
            eventType, normalizedTenant, correlation, sourceService);
    }

    public async Task<IReadOnlyList<AuditEntryDto>> QueryAsync(AuditQuery query, CancellationToken ct = default)
    {
        var limit = Math.Clamp(query.Limit, 1, 500);

        IReadOnlyList<AuditEntry> entries;

        if (!string.IsNullOrWhiteSpace(query.CorrelationId))
        {
            entries = await repository.QueryByCorrelationAsync(query.CorrelationId!, limit, ct);
        }
        else if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            entries = await repository.QueryByEventTypeAsync(query.EventType!, limit, ct);
        }
        else
        {
            var tenant = Tenants.Normalize(query.TenantId);
            entries = await repository.QueryByTenantAsync(tenant, query.From, query.To, limit, ct);
        }

        return entries.Select(AuditEntryDto.From).ToList();
    }
}
