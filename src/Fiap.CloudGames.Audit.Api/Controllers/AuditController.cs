using Fiap.CloudGames.Audit.Application.Dtos;
using Fiap.CloudGames.Audit.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.CloudGames.Audit.Api.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditController(IAuditService audit) : ControllerBase
{
    /// <summary>
    /// Query audit entries by tenant (with optional time range), correlation id, or event type.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditEntryDto>>> Query(
        [FromQuery] string? tenantId,
        [FromQuery] string? correlationId,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var query = new AuditQuery(tenantId, correlationId, eventType, from, to, limit);
        var result = await audit.QueryAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("correlation/{correlationId}")]
    public async Task<ActionResult<IReadOnlyList<AuditEntryDto>>> ByCorrelation(
        string correlationId,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var query = new AuditQuery(null, correlationId, null, null, null, limit);
        var result = await audit.QueryAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("event/{eventType}")]
    public async Task<ActionResult<IReadOnlyList<AuditEntryDto>>> ByEventType(
        string eventType,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var query = new AuditQuery(null, null, eventType, null, null, limit);
        var result = await audit.QueryAsync(query, ct);
        return Ok(result);
    }
}
