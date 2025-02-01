using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    DateTime ArrivalDate,
    DateTime DepartureDate,
    uint Adults,
    uint Children
) : IRequest<Result<Guid>>;