using Acme.Domain.Events;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Acme.Infrastructure.Events;

internal sealed partial class TransactionalOutbox(
    IAmazonDb amazonDb,
    IOptionsSnapshot<OutboxOptions> options,
    ILogger<TransactionalOutbox> logger
) : ITransactionalOutbox
{
    public void Publish<TPayload>(string eventName, TPayload payload, string? topic = null)
    {
        var message = Message.Create(eventName, payload);
        LogPublish(logger, message);

        amazonDb.Put(new PutItemRequest
        {
            Item = MessageMapper.ToMap(message),
            TableName = options.Value.TableName
        });
    }
}