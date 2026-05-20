using FluentAssertions;
using Fiap.CloudGames.Audit.Application.Services;
using Fiap.CloudGames.Audit.Domain.Entities;
using Fiap.CloudGames.Audit.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Fiap.CloudGames.Audit.Tests;

public class AuditServiceTests
{
    [Fact]
    public async Task RecordAsync_normalizes_unknown_tenant_and_persists()
    {
        var repo = new Mock<IAuditRepository>();
        var svc = new AuditService(repo.Object, NullLogger<AuditService>.Instance);

        await svc.RecordAsync(
            payload: new { name = "bruno" },
            eventType: "TestEvent",
            sourceService: "users",
            aggregateId: "agg-1",
            correlationId: "corr-1",
            tenantId: "WhoKnows");

        repo.Verify(r => r.AddAsync(
            It.Is<AuditEntry>(e =>
                e.TenantId == "unknown" &&
                e.EventType == "TestEvent" &&
                e.CorrelationId == "corr-1" &&
                e.AggregateId == "agg-1" &&
                e.SourceService == "users"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecordAsync_accepts_FIAP_tenant_unchanged()
    {
        var repo = new Mock<IAuditRepository>();
        var svc = new AuditService(repo.Object, NullLogger<AuditService>.Instance);

        await svc.RecordAsync(
            payload: new { x = 1 },
            eventType: "T",
            sourceService: "src",
            aggregateId: null,
            correlationId: null,
            tenantId: "FIAP");

        repo.Verify(r => r.AddAsync(
            It.Is<AuditEntry>(e => e.TenantId == "FIAP" && !string.IsNullOrWhiteSpace(e.CorrelationId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
