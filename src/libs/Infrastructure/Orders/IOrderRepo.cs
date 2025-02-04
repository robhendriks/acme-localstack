using Acme.Domain.Orders;

namespace Acme.Infrastructure.Orders;

public interface IOrderRepo
{
    void Create(Order order);
}