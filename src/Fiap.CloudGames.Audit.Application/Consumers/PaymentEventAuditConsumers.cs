using Fiap.CloudGames.Application.Payments.Events;
using Fiap.CloudGames.Audit.Application.Services;
using Microsoft.Extensions.Logging;

namespace Fiap.CloudGames.Audit.Application.Consumers;

public sealed class PaymentLinkGeneratedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<PaymentLinkGeneratedEvent>> logger)
    : AuditConsumerBase<PaymentLinkGeneratedEvent>(audit, logger)
{
    protected override string SourceService => "payments";
    protected override string? AggregateIdFor(PaymentLinkGeneratedEvent m) => m.OrderId.ToString();
}

public sealed class PaymentSucceededAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<PaymentSucceededEvent>> logger)
    : AuditConsumerBase<PaymentSucceededEvent>(audit, logger)
{
    protected override string SourceService => "payments";
    protected override string? AggregateIdFor(PaymentSucceededEvent m) => m.OrderId.ToString();
}

public sealed class PaymentFailedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<PaymentFailedEvent>> logger)
    : AuditConsumerBase<PaymentFailedEvent>(audit, logger)
{
    protected override string SourceService => "payments";
    protected override string? AggregateIdFor(PaymentFailedEvent m) => m.OrderId.ToString();
}

public sealed class PaymentRefundedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<PaymentRefundedEvent>> logger)
    : AuditConsumerBase<PaymentRefundedEvent>(audit, logger)
{
    protected override string SourceService => "payments";
    protected override string? AggregateIdFor(PaymentRefundedEvent m) => m.OrderId.ToString();
}

public sealed class PaymentRefundFailedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<PaymentRefundFailedEvent>> logger)
    : AuditConsumerBase<PaymentRefundFailedEvent>(audit, logger)
{
    protected override string SourceService => "payments";
    protected override string? AggregateIdFor(PaymentRefundFailedEvent m) => m.OrderId.ToString();
}
