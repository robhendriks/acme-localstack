using Acme.Domain.Orders;

namespace Acme.Persistence.Orders;

public interface IOrderRepository
{
    void Create(Order order);
}