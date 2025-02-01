using Acme.Domain.Orders;
using Acme.Persistence.Orders;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Queries.GetOrder;

public sealed class GetOrderQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderQuery, Result<Order>>
{
    public async Task<Result<Order>> Handle(GetOrderQuery request, CancellationToken cancellationToken) =>
        await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
}