using System.Globalization;
using Acme.Domain.Orders;
using Acme.Infrastructure.Storage;
using Amazon.DynamoDBv2.Model;
using FluentResults;
using Microsoft.Extensions.Options;

namespace Acme.Persistence.Orders;

internal sealed class OrderRepository(
    IAmazonDatabase database,
    IOptions<OrderTableOptions> options
) : IOrderRepository
{
    public async Task<Result<Order>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var result = await database.GetAsync(options.Value.TableName, "id", orderId.ToString("D"), cancellationToken);
        if (result.IsFailed)
        {
            return Result.Fail("Nooooo").WithReasons(result.Errors);
        }

        if (!result.Value.IsItemSet)
        {
            return Result.Fail("NOOOOOOOOOOOOOOOOOOO!");
        }

        return OrderMapper.FromMap(result.Value.Item);
    }

    public void Create(Order order)
    {
        database.Put(new PutItemRequest
        {
            TableName = options.Value.TableName,
            Item = OrderMapper.ToMap(order)
        });
    }
}