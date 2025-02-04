using System.Globalization;
using Acme.Domain.Events;
using Amazon.DynamoDBv2.Model;

namespace Acme.Infrastructure.Events;

public static class DomainEventMapper
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
            S = domainEvent.CreatedAt.ToString("O", CultureInfo.InvariantCulture)
        }
    };

    public static IDomainEvent FromMap(Dictionary<string, AttributeValue> map) => new DomainEvent
    {
        Id = Guid.Parse(map["id"].S),
        EventName = map["eventName"].S,
        Content = map["content"].S,
        ContentHash = map["contentHash"].S,
        Topic = map["topic"].S,
        CreatedAt = DateTime.Parse(
            map["createdAt"].S,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal
        )
    };
}