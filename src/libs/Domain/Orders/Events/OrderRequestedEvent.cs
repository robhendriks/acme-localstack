namespace Acme.Domain.Orders.Events;

public sealed record OrderRequestedEvent(Guid Id)
{
    public const string EventName = "OrderRequested";
}