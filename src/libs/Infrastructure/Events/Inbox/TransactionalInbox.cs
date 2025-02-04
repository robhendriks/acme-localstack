using System.Globalization;
using Acme.Domain.Events;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace Acme.Infrastructure.Events.Inbox;

internal sealed class TransactionalInbox(
    IAmazonDb amazonDb,
    IOptionsSnapshot<InboxOptions> options
)
    : ITransactionalInbox
{
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
                    ":t",
                    new AttributeValue { N = AmazonDbUtil.TimeToLive().ToString(CultureInfo.InvariantCulture) }
                }
            },
            UpdateExpression = "SET #T = :t",
        });
    }
}