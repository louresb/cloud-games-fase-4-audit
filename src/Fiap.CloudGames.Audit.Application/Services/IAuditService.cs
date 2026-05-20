using Fiap.CloudGames.Audit.Application.Dtos;
using Fiap.CloudGames.Audit.Domain.Entities;

namespace Fiap.CloudGames.Audit.Application.Services;

public interface IAuditService
{
    Task RecordAsync<TEvent>(
        TEvent payload,
        string eventType,
        string sourceService,
        string? aggregateId,
        string? correlationId,
        string? tenantId,
        CancellationToken ct = default) where TEvent : class;

    Task<IReadOnlyList<AuditEntryDto>> QueryAsync(AuditQuery query, CancellationToken ct = default);
}
