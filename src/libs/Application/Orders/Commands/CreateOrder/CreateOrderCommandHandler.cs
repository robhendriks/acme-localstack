using Acme.Domain.Orders;
using Acme.Infrastructure.Storage;
using Acme.Persistence.Orders;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(IOrderRepository orderRepository, IAmazonDatabase amazonDatabase)
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(Guid.NewGuid());

        // Begin transaction
        orderRepository.Create(order);

        // Commit transaction
        await amazonDatabase.SaveChangesAsync(cancellationToken);

        return Result.Ok(order.Id);
    }
}