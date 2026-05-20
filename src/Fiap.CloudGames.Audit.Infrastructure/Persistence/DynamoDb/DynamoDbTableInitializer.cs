using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fiap.CloudGames.Audit.Infrastructure.Persistence.DynamoDb;

public sealed class DynamoDbTableInitializer(
    IAmazonDynamoDB client,
    DynamoDbAuditOptions options,
    ILogger<DynamoDbTableInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        if (!options.AutoCreateTable) return;

        try
        {
            var list = await client.ListTablesAsync(ct);
            if (list.TableNames.Contains(options.TableName))
            {
                logger.LogInformation("DynamoDB table {Table} already exists", options.TableName);
                return;
            }

            logger.LogInformation("Creating DynamoDB table {Table}", options.TableName);

            await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = options.TableName,
                AttributeDefinitions =
                [
                    new() { AttributeName = "TenantId", AttributeType = ScalarAttributeType.S },
                    new() { AttributeName = "SortKey", AttributeType = ScalarAttributeType.S },
                    new() { AttributeName = "CorrelationId", AttributeType = ScalarAttributeType.S },
                    new() { AttributeName = "EventType", AttributeType = ScalarAttributeType.S }
                ],
                KeySchema =
                [
                    new() { AttributeName = "TenantId", KeyType = KeyType.HASH },
                    new() { AttributeName = "SortKey", KeyType = KeyType.RANGE }
                ],
                GlobalSecondaryIndexes =
                [
                    new()
                    {
                        IndexName = "gsi_correlation",
                        KeySchema =
                        [
                            new() { AttributeName = "CorrelationId", KeyType = KeyType.HASH },
                            new() { AttributeName = "SortKey", KeyType = KeyType.RANGE }
                        ],
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    },
                    new()
                    {
                        IndexName = "gsi_event_type",
                        KeySchema =
                        [
                            new() { AttributeName = "EventType", KeyType = KeyType.HASH },
                            new() { AttributeName = "SortKey", KeyType = KeyType.RANGE }
                        ],
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
                ],
                BillingMode = BillingMode.PAY_PER_REQUEST
            }, ct);

            logger.LogInformation("DynamoDB table {Table} created", options.TableName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not auto-create DynamoDB table {Table}; assuming external provisioning", options.TableName);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
