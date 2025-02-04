using System.Globalization;
using Acme.Domain.Events;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace Acme.Infrastructure.Events.Inbox;

internal sealed partial class TransactionalInbox(
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
            Key = new Dictionary<string, AttributeValue>()
            {
                { "id", new AttributeValue { S = domainEvent.Id.ToString("D") } },
            },
            UpdateExpression = "SET #time_to_live = :time_to_live",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {
                    ":time_to_live",
                    new AttributeValue { N = AmazonDbUtil.CalculateTtl().ToString(CultureInfo.InvariantCulture) }
                }
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#time_to_live", "ttl" }
            }
        });
    }
}