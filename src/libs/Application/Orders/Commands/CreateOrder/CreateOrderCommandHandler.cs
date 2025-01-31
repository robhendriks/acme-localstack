using Acme.Domain.Orders;
using Acme.Domain.Orders.Events;
using Acme.Infrastructure.Storage;
using Acme.Persistence.InboxOutbox;
using Acme.Persistence.Orders;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IOutboxRepository outboxRepository,
    IAmazonDatabase amazonDatabase)
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(Guid.NewGuid());

        // Begin transaction
        orderRepository.Create(order);

        outboxRepository.Create(
            OrderRequestedEvent.EventName,
            new OrderRequestedEvent(order.Id)
        );

        // Commit transaction
        var result = await amazonDatabase.SaveChangesAsync(cancellationToken);
        if (result.IsFailed)
        {
            return result;
        }

        return Result.Ok(order.Id);
    }
}