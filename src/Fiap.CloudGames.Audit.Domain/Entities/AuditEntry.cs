namespace Fiap.CloudGames.Audit.Domain.Entities;

public sealed class AuditEntry
{
    public required string TenantId { get; init; }
    public required string Id { get; init; }
    public required string EventType { get; init; }
    public required string SourceService { get; init; }
    public required string CorrelationId { get; init; }
    public string? AggregateId { get; init; }
    public required string PayloadJson { get; init; }
    public required DateTime CreatedAt { get; init; }

    public string SortKey => $"{CreatedAt:O}#{Id}";

    public static AuditEntry Create(
        string tenantId,
        string eventType,
        string sourceService,
        string correlationId,
        string? aggregateId,
        string payloadJson) =>
        new()
        {
            TenantId = tenantId,
            Id = Guid.NewGuid().ToString("N"),
            EventType = eventType,
            SourceService = sourceService,
            CorrelationId = correlationId,
            AggregateId = aggregateId,
            PayloadJson = payloadJson,
            CreatedAt = DateTime.UtcNow
        };
}
