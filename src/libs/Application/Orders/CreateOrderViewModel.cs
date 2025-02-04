namespace Acme.Application.Orders;

public sealed class CreateOrderViewModel
{
    public required Guid Id { get; init; }

    public required string Note { get; set; }
}