using Acme.Domain.Events;

namespace Acme.Infrastructure.Events.Inbox;

public interface ITransactionalInbox
{
    void Consume(IDomainEvent domainEvent);
}