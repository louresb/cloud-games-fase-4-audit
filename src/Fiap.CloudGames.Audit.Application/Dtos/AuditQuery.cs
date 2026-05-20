namespace Fiap.CloudGames.Audit.Application.Dtos;

public sealed record AuditQuery(
    string? TenantId,
    string? CorrelationId,
    string? EventType,
    DateTime? From,
    DateTime? To,
    int Limit = 50);
