namespace Acme.Domain.Events;

public interface IHasDomainEvents
{
    List<IDomainEvent> DomainEvents { get; }
}