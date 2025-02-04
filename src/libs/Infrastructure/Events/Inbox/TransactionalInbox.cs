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
            UpdateExpression = "SET ttl = :ttl",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {
                    ":ttl",
                    new AttributeValue { N = AmazonDbUtil.CalculateTtl().ToString(CultureInfo.InvariantCulture) }
                },
            }
        });
    }
}