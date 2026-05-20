using Fiap.CloudGames.Application.Users.Events;
using Fiap.CloudGames.Audit.Application.Services;
using Microsoft.Extensions.Logging;

namespace Fiap.CloudGames.Audit.Application.Consumers;

public sealed class UserSignedUpAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<UserSignedUpEvent>> logger)
    : AuditConsumerBase<UserSignedUpEvent>(audit, logger)
{
    protected override string SourceService => "users";
    protected override string? AggregateIdFor(UserSignedUpEvent m) => m.Id.ToString();
}

public sealed class UserEmailConfirmedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<UserEmailConfirmedEvent>> logger)
    : AuditConsumerBase<UserEmailConfirmedEvent>(audit, logger)
{
    protected override string SourceService => "users";
    protected override string? AggregateIdFor(UserEmailConfirmedEvent m) => m.Id.ToString();
}

public sealed class UserInvitedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<UserInvitedEvent>> logger)
    : AuditConsumerBase<UserInvitedEvent>(audit, logger)
{
    protected override string SourceService => "users";
    protected override string? AggregateIdFor(UserInvitedEvent m) => m.Id.ToString();
}

public sealed class UserFirstAccessedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<UserFirstAccessedEvent>> logger)
    : AuditConsumerBase<UserFirstAccessedEvent>(audit, logger)
{
    protected override string SourceService => "users";
    protected override string? AggregateIdFor(UserFirstAccessedEvent m) => m.Id.ToString();
}

public sealed class UserForgotPasswordAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<UserForgotPasswordEvent>> logger)
    : AuditConsumerBase<UserForgotPasswordEvent>(audit, logger)
{
    protected override string SourceService => "users";
    protected override string? AggregateIdFor(UserForgotPasswordEvent m) => m.Id.ToString();
}

public sealed class UserPasswordResetedAuditConsumer(IAuditService audit, ILogger<AuditConsumerBase<UserPasswordResetedEvent>> logger)
    : AuditConsumerBase<UserPasswordResetedEvent>(audit, logger)
{
    protected override string SourceService => "users";
    protected override string? AggregateIdFor(UserPasswordResetedEvent m) => m.Id.ToString();
}
