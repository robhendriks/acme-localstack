using Acme.Domain.Orders;
using Acme.Infrastructure.Orders;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands;

public sealed class CreateOrderCommandHandler(IOrderRepo orderRepo)
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderViewModel>>
{
    public Task<Result<CreateOrderViewModel>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = Order.Create(request.Note);

        orderRepo.Create(order);

        var orderViewModel = new CreateOrderViewModel { Id = order.Id, Note = order.Note };

        return Task.FromResult(Result.Ok(orderViewModel));
    }
}