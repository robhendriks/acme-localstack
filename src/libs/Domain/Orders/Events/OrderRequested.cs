namespace Acme.Domain.Orders.Events;

public sealed record OrderRequested(Guid OrderId)
{
    public const string EventName = "Acme.OrderRequested";
}