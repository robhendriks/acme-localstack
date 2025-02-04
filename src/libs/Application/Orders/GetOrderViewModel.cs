namespace Acme.Application.Orders;

public sealed class GetOrderViewModel
{
    public required Guid Id { get; init; }
    public required string Note { get; init; }
}