namespace Acme.Infrastructure.Events;

public interface ITransactionalOutbox
{
    void Publish<TPayload>(string eventName, TPayload payload, string? topic = null);
}