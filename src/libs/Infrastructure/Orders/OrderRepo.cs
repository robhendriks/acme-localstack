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

    public Task<Result<Order>> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Fail<Order>("NO!"));
    }
}