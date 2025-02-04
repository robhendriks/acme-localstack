using Acme.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Acme.Infrastructure.Events;

internal sealed partial class TransactionalOutbox
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Publishing {@Message} to outbox"
    )]
    public static partial void LogPublish(ILogger logger, Message message);
}