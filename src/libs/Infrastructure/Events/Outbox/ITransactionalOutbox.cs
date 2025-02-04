using Acme.Domain.Events;

namespace Acme.Infrastructure.Events.Outbox;

public interface ITransactionalOutbox
{
    void PublishAll(IHasDomainEvents hasDomainEvents);
    void Consume(IDomainEvent domainEvent);
}