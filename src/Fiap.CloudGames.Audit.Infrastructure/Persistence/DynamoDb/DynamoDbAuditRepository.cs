using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Fiap.CloudGames.Audit.Domain.Entities;
using Fiap.CloudGames.Audit.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Fiap.CloudGames.Audit.Infrastructure.Persistence.DynamoDb;

public sealed class DynamoDbAuditRepository(
    IAmazonDynamoDB client,
    DynamoDbAuditOptions options,
    ILogger<DynamoDbAuditRepository> logger) : IAuditRepository
{
    private readonly ILogger<DynamoDbAuditRepository> _logger = logger;
    private const string PK = "TenantId";
    private const string SK = "SortKey";
    private const string GsiCorrelation = "gsi_correlation";
    private const string GsiEventType = "gsi_event_type";

    public async Task AddAsync(AuditEntry entry, CancellationToken ct = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            [PK] = new() { S = entry.TenantId },
            [SK] = new() { S = entry.SortKey },
            ["Id"] = new() { S = entry.Id },
            ["EventType"] = new() { S = entry.EventType },
            ["SourceService"] = new() { S = entry.SourceService },
            ["CorrelationId"] = new() { S = entry.CorrelationId },
            ["PayloadJson"] = new() { S = entry.PayloadJson },
            ["CreatedAt"] = new() { S = entry.CreatedAt.ToString("O") }
        };

        if (!string.IsNullOrWhiteSpace(entry.AggregateId))
        {
            item["AggregateId"] = new AttributeValue { S = entry.AggregateId };
        }

        await client.PutItemAsync(new PutItemRequest
        {
            TableName = options.TableName,
            Item = item
        }, ct);

        _logger.LogDebug("AuditEntry persisted: {EventType} {Id}", entry.EventType, entry.Id);
    }

    public async Task<IReadOnlyList<AuditEntry>> QueryByTenantAsync(
        string tenantId, DateTime? from, DateTime? to, int limit, CancellationToken ct = default)
    {
        var req = new QueryRequest
        {
            TableName = options.TableName,
            KeyConditionExpression = "#pk = :pk",
            ExpressionAttributeNames = new() { { "#pk", PK } },
            ExpressionAttributeValues = new() { { ":pk", new AttributeValue { S = tenantId } } },
            ScanIndexForward = false,
            Limit = limit
        };

        if (from.HasValue || to.HasValue)
        {
            var fromKey = from?.ToString("O") ?? DateTime.MinValue.ToString("O");
            var toKey = to?.ToString("O") ?? DateTime.MaxValue.ToString("O");
            req.KeyConditionExpression = "#pk = :pk AND #sk BETWEEN :fromKey AND :toKey";
            req.ExpressionAttributeNames["#sk"] = SK;
            req.ExpressionAttributeValues[":fromKey"] = new AttributeValue { S = fromKey };
            req.ExpressionAttributeValues[":toKey"] = new AttributeValue { S = toKey + "￿" };
        }

        var resp = await client.QueryAsync(req, ct);
        return resp.Items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<AuditEntry>> QueryByCorrelationAsync(string correlationId, int limit, CancellationToken ct = default)
    {
        var req = new QueryRequest
        {
            TableName = options.TableName,
            IndexName = GsiCorrelation,
            KeyConditionExpression = "CorrelationId = :c",
            ExpressionAttributeValues = new() { { ":c", new AttributeValue { S = correlationId } } },
            ScanIndexForward = true,
            Limit = limit
        };

        var resp = await client.QueryAsync(req, ct);
        return resp.Items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<AuditEntry>> QueryByEventTypeAsync(string eventType, int limit, CancellationToken ct = default)
    {
        var req = new QueryRequest
        {
            TableName = options.TableName,
            IndexName = GsiEventType,
            KeyConditionExpression = "EventType = :e",
            ExpressionAttributeValues = new() { { ":e", new AttributeValue { S = eventType } } },
            ScanIndexForward = false,
            Limit = limit
        };

        var resp = await client.QueryAsync(req, ct);
        return resp.Items.Select(Map).ToList();
    }

    private static AuditEntry Map(Dictionary<string, AttributeValue> item) => new()
    {
        TenantId = item[PK].S,
        Id = item.TryGetValue("Id", out var id) ? id.S : Guid.NewGuid().ToString("N"),
        EventType = item.TryGetValue("EventType", out var et) ? et.S : "Unknown",
        SourceService = item.TryGetValue("SourceService", out var src) ? src.S : "unknown",
        CorrelationId = item.TryGetValue("CorrelationId", out var cid) ? cid.S : Guid.NewGuid().ToString(),
        AggregateId = item.TryGetValue("AggregateId", out var aid) ? aid.S : null,
        PayloadJson = item.TryGetValue("PayloadJson", out var p) ? p.S : "{}",
        CreatedAt = item.TryGetValue("CreatedAt", out var ts) && DateTime.TryParse(ts.S, out var dt) ? dt.ToUniversalTime() : DateTime.UtcNow
    };
}

public sealed class DynamoDbAuditOptions
{
    public string TableName { get; set; } = "cloud-games-audit-events";
    public string? ServiceUrl { get; set; }
    public string Region { get; set; } = "us-east-1";
    public bool AutoCreateTable { get; set; } = true;
}
