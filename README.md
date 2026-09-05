# FIAP Cloud Games - Audit Service (Fase 4)

Serviço centralizado de auditoria que consome eventos de integração dos demais domínios e mantém um histórico append-only no Amazon DynamoDB. Os registros podem ser consultados por tenant, tipo de evento ou identificador de correlação.

## Objetivos

- Manter uma trilha de alterações e eventos relevantes.
- Isolar os registros por `TenantId`.
- Permitir a reconstrução de fluxos distribuídos por `CorrelationId`.
- Preservar o payload original e os metadados de origem.

## Arquitetura

```text
Users -------+
Catalog -----+--> RabbitMQ --> Audit Service --> DynamoDB
Payments ----+                       |
                                     +--> REST API
```

O serviço usa RabbitMQ com MassTransit para consumir eventos. No ambiente local, o DynamoDB é executado pelo LocalStack; na AWS, a tabela é provisionada pelo Terraform do repositório de orquestração.

## Tabela DynamoDB

| Atributo | Uso |
|---|---|
| `TenantId` | Partition key |
| `SortKey` | Timestamp ISO + identificador, permitindo ordenação |
| `EventType` | Partition key do índice `gsi_event_type` |
| `CorrelationId` | Partition key do índice `gsi_correlation` |
| `SourceService` | Serviço que originou o evento |
| `AggregateId` | Entidade relacionada |
| `PayloadJson` | Payload original |
| `CreatedAt` | Data de criação |

A tabela usa cobrança sob demanda (`PAY_PER_REQUEST`).

## Endpoints

| Método | Endpoint | Uso |
|---|---|---|
| `GET` | `/api/audit?tenantId=FIAP&from=...&to=...&limit=50` | Consulta eventos recentes por tenant |
| `GET` | `/api/audit/correlation/{correlationId}` | Reconstrói um fluxo distribuído |
| `GET` | `/api/audit/event/{eventType}` | Filtra pelo tipo do evento |
| `GET` | `/health/live` | Liveness probe |
| `GET` | `/health/ready` | Readiness probe |

## Execução local

Inicie o LocalStack, o RabbitMQ e o Loki pelo [repositório de orquestração](https://github.com/louresb/cloud-games-fase-4-orchestration-aws):

```bash
docker compose -f ../cloud-games-fase-4-orchestration-aws/docker-compose.infra.yaml up -d
```

Depois execute a API:

```bash
dotnet run --project src/Fiap.CloudGames.Audit.Api
```

Com `DynamoDb:AutoCreateTable=true`, o serviço cria a tabela local na primeira execução.

## Kubernetes e AWS

Os manifests em `k8s/` definem Deployment, Service, ConfigMap, Secret e HPA. O deploy usa rolling update com `maxUnavailable: 0` e `maxSurge: 1`.

Na AWS:

- A tabela é definida em `terraform/dynamodb.tf` no repositório de orquestração.
- O acesso ao DynamoDB usa IAM Roles for Service Accounts (IRSA).
- Secrets são obtidos do AWS Secrets Manager por External Secrets.
- A imagem é publicada no Amazon ECR e pode ser implantada no Amazon EKS pelo workflow de deploy.

## Repositórios relacionados

- [Orquestração](https://github.com/louresb/cloud-games-fase-4-orchestration-aws)
- [Users](https://github.com/louresb/cloud-games-fase-4-users)
- [Catalog](https://github.com/louresb/cloud-games-fase-4-catalog)
- [Payments](https://github.com/louresb/cloud-games-fase-4-payments)
- [Notifications](https://github.com/louresb/cloud-games-fase-4-notifications)
