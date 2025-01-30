using Acme.Domain.Orders;
using Acme.Infrastructure.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Acme.Persistence.Orders;

internal sealed class OrderRepository(
    IAmazonDatabase database,
    IOptions<OrderTableOptions> options,
    IConfiguration configuration) : IOrderRepository
{
    public void Create(Order order)
    {
        var cfg = configuration;

        database.Put(new PutItemRequest
        {
            TableName = options.Value.TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["id"] = new()
                {
                    S = order.Id.ToString("D")
                },
                ["test"] = new()
                {
                    S = "hello"
                }
            }
        });
    }
}