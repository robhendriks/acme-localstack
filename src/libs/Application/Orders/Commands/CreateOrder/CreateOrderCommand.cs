using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand : IRequest<Result<Guid>>;