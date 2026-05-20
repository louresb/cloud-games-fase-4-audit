using Fiap.CloudGames.Audit.Domain.Entities;

namespace Fiap.CloudGames.Audit.Domain.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<AuditEntry>> QueryByTenantAsync(
        string tenantId,
        DateTime? from,
        DateTime? to,
        int limit,
        CancellationToken ct = default);

    Task<IReadOnlyList<AuditEntry>> QueryByCorrelationAsync(
        string correlationId,
        int limit,
        CancellationToken ct = default);

    Task<IReadOnlyList<AuditEntry>> QueryByEventTypeAsync(
        string eventType,
        int limit,
        CancellationToken ct = default);
}
