namespace Acme.Persistence.InboxOutbox;

public interface IOutboxRepository
{
    void Create<TMessage>(string eventName, TMessage payload, string? topic = null);
}