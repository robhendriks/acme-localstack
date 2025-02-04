using Acme.Domain.Events;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
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
}