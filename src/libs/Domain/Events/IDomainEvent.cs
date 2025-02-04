namespace Acme.Domain.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    string EventName { get; }
    string Content { get; }
    string ContentHash { get; }
    string Topic { get; }
    DateTime CreatedAt { get; }

    T ToT<T>();
}