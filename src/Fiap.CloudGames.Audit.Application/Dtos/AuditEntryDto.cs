using Fiap.CloudGames.Audit.Domain.Entities;

namespace Fiap.CloudGames.Audit.Application.Dtos;

public sealed record AuditEntryDto(
    string TenantId,
    string Id,
    string EventType,
    string SourceService,
    string CorrelationId,
    string? AggregateId,
    string PayloadJson,
    DateTime CreatedAt)
{
    public static AuditEntryDto From(AuditEntry e) =>
        new(e.TenantId, e.Id, e.EventType, e.SourceService, e.CorrelationId, e.AggregateId, e.PayloadJson, e.CreatedAt);
}
