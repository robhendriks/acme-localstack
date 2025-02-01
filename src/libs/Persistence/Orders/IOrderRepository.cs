using Acme.Domain.Orders;
using FluentResults;

namespace Acme.Persistence.Orders;

public interface IOrderRepository
{
    Task<Result<Order>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    void Create(Order order);
}