using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Queries;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<Result<GetOrderViewModel>>;