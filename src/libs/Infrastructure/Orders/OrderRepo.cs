using Acme.Domain.Orders;
using Acme.Infrastructure.Events.Outbox;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2.Model;
using FluentResults;
using Microsoft.Extensions.Options;

namespace Acme.Infrastructure.Orders;

internal sealed class OrderRepo(
    IAmazonDb amazonDb,
    ITransactionalOutbox outbox,
    IOptionsSnapshot<OrderOptions> options) : IOrderRepo
{
    public void Create(Order order)
    {
        amazonDb.Put(new PutItemRequest
        {
            Item = OrderMapper.ToMap(order),
            TableName = options.Value.TableName
        });

        outbox.PublishAll(order);
    }

    public async Task<Result<Order?>> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var request = new GetItemRequest
        {
            TableName = options.Value.TableName,
            Key = { ["id"] = new AttributeValue { S = orderId.ToString("D") } }
        };

        var result = await amazonDb.GetAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors[0]);
        }

        if (!result.Value.IsItemSet)
        {
            return Result.Ok<Order?>(null);
        }

        return OrderMapper.FromMap(result.Value.Item);
    }
}