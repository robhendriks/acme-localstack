using Acme.Domain.InboxOutbox;
using Acme.Infrastructure.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace Acme.Persistence.InboxOutbox;

internal sealed class OutboxRepository(
    IAmazonDatabase database,
    IOptions<OutboxTableOptions> options)
    : IOutboxRepository
{
    public void Create<TPayload>(string eventName, TPayload payload, string? topic = null)
    {
        var message = Message.Create(payload, eventName, topic);

        database.Put(new PutItemRequest
        {
            TableName = options.Value.TableName,
            Item = MessageMapper.ToMap(message)
        });
    }
}