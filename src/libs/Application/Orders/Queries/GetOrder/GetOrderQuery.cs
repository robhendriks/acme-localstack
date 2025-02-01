using Acme.Domain.Orders;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<Result<Order>>;