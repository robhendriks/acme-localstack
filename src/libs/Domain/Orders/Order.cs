using Acme.Domain.Events;
using Acme.Domain.Orders.Events;

namespace Acme.Domain.Orders;

public sealed class Order : IHasDomainEvents
{
    public required Guid Id { get; init; }
    public required string Note { get; init; }
    public required List<IDomainEvent> DomainEvents { get; init; }

    public static Order Create(string note)
    {
        var orderId = Guid.NewGuid();

        return new Order
        {
            Id = orderId,
            Note = note,
            DomainEvents =
            [
                DomainEvent.Create(OrderRequested.EventName, new OrderRequested(orderId))
            ]
        };
    }
}