using System.Globalization;
using Acme.Domain.Events;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace Acme.Infrastructure.Events.Outbox;

internal sealed partial class TransactionalOutbox(
    IAmazonDb amazonDb,
    IOptionsSnapshot<OutboxOptions> options
) : ITransactionalOutbox
{
    public void PublishAll(IHasDomainEvents hasDomainEvents)
    {
        foreach (var domainEvent in hasDomainEvents.DomainEvents)
        {
            amazonDb.Put(new PutItemRequest
            {
                Item = DomainEventMapper.ToMap(domainEvent),
                TableName = options.Value.TableName
            });
        }

        hasDomainEvents.DomainEvents.Clear();
    }

    public void Consume(IDomainEvent domainEvent)
    {
        amazonDb.Update(new UpdateItemRequest
        {
            TableName = options.Value.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = domainEvent.Id.ToString("D") } },
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#T", "ttl" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {
                    ":t", new AttributeValue { N = AmazonDbUtil.TimeToLive().ToString(CultureInfo.InvariantCulture) }
                }
            },
            UpdateExpression = "SET #T = :t",
        });
    }
}