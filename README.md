# Fiap Cloud Games — Audit Service (Phase 4)

Centralized, append-only audit log of integration events consumed from the other services. Persisted in **Amazon DynamoDB** (single-table design) and queryable via REST API.

## Why

- Forensic trail for compliance (LGPD).
- Multi-tenant per `X-Tenant-Id` (FIAP / Alura / PM3).
- Distributed-tracing companion: query by `CorrelationId` to reconstruct a user journey across services.

## Architecture

```
+---------+   +---------+   +---------+
| Users   |   | Catalog |   | Payments |
+---------+   +---------+   +---------+
     |             |             |
     +-------------+-------------+
                   |
              fanout exchange (RabbitMQ / SNS topic)
                   |
            +------v------+
            | audit-svc   |
            | (consumer)  |
            +------+------+
                   |
            +------v------+
            |  DynamoDB   |  cloud-games-audit-events
            +-------------+
                   |
            +------v------+
            | audit-svc   |
            | (REST API)  |  GET /api/audit
            +-------------+
```

## DynamoDB table

| Attribute | Type | Notes |
|---|---|---|
| `TenantId` | PK (HASH) | FIAP / Alura / PM3 / unknown |
| `SortKey` | SK (RANGE) | `<ISO timestamp>#<id>` — newest first when ScanIndexForward=false |
| `EventType` | GSI `gsi_event_type` HASH | filter by event |
| `CorrelationId` | GSI `gsi_correlation` HASH | trace a request across services |
| `SourceService`, `AggregateId`, `PayloadJson`, `CreatedAt` | attributes | raw event payload + metadata |

Billing: PAY_PER_REQUEST (on-demand).

## Endpoints

| Verb | Path | Use |
|---|---|---|
| GET | `/api/audit?tenantId=FIAP&from=...&to=...&limit=50` | newest-first scan by tenant |
| GET | `/api/audit/correlation/{correlationId}` | trace a single request |
| GET | `/api/audit/event/{eventType}` | filter by event name |
| GET | `/health/live` / `/health/ready` | K8s probes |
| GET | `/swagger` | OpenAPI UI (dev only) |

## Local dev

```bash
# 1. start LocalStack (DynamoDB) + RabbitMQ + Loki
docker compose -f ../cloud-games-fase-4-orchestration-aws/docker-compose.infra.yaml up -d

# 2. run the API
dotnet run --project src/Fiap.CloudGames.Audit.Api
```

The service auto-creates the DynamoDB table on first run (`DynamoDb:AutoCreateTable=true`).

## Kubernetes

```bash
kubectl apply -f k8s/audit-configmap.yaml
kubectl apply -f k8s/audit-secret.yaml
kubectl apply -f k8s/audit-service.yaml
kubectl apply -f k8s/audit-deployment.yaml
kubectl apply -f k8s/audit-hpa.yaml
```

Rolling Update is enforced (`maxUnavailable: 0`, `maxSurge: 1`); HPA scales 2–6 pods on CPU/mem.

## AWS deployment notes

- DynamoDB table provisioned by Terraform module `terraform/modules/dynamodb` in the orchestration repo. Set `DynamoDb__AutoCreateTable=false` and remove `DynamoDb__ServiceUrl` in production.
- IAM permissions via IRSA (`eks.amazonaws.com/role-arn` annotation on the `audit-sa` service account). Policy: `dynamodb:PutItem`, `dynamodb:Query` on the table + GSIs.
- Messaging in AWS uses `MESSAGING_PROVIDER=SQS` + MassTransit AmazonSQS transport.
