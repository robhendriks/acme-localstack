using Acme.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Acme.Infrastructure.Events.Outbox;

internal sealed partial class TransactionalOutbox
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Publishing {@DomainEvent} to outbox"
    )]
    public static partial void LogPublish(ILogger logger, IDomainEvent domainEvent);
}