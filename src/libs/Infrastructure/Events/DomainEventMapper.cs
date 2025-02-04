using Acme.Domain.Events;
using Amazon.DynamoDBv2.Model;

namespace Acme.Infrastructure.Events;

internal static class DomainEventMapper
{
    public static Dictionary<string, AttributeValue> ToMap(IDomainEvent domainEvent) => new()
    {
        ["id"] = new AttributeValue
        {
            S = domainEvent.Id.ToString("D")
        },
        ["eventName"] = new AttributeValue
        {
            S = domainEvent.EventName
        },
        ["topic"] = new AttributeValue
        {
            S = domainEvent.Topic
        },
        ["content"] = new AttributeValue
        {
            S = domainEvent.Content
        },
        ["contentHash"] = new AttributeValue
        {
            S = domainEvent.ContentHash
        },
        ["createdAt"] = new AttributeValue
        {
            S = domainEvent.CreatedAt.ToString("O")
        }
    };
}