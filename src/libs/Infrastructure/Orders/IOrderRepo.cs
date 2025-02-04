using Acme.Domain.Orders;
using FluentResults;

namespace Acme.Infrastructure.Orders;

public interface IOrderRepo
{
    void Create(Order order);
    Task<Result<Order>> GetAsync(Guid orderId, CancellationToken cancellationToken = default);
}