using System.Globalization;
using Acme.Domain.InboxOutbox;
using Amazon.DynamoDBv2.Model;

namespace Acme.Persistence.InboxOutbox;

public static class MessageMapper
{
    public static Dictionary<string, AttributeValue> ToMap(Message message) => new()
    {
        ["id"] = new AttributeValue
        {
            S = message.Id.ToString("D")
        },
        ["topic"] = new AttributeValue
        {
            S = message.Topic
        },
        ["eventName"] = new AttributeValue
        {
            S = message.EventName
        },
        ["content"] = new AttributeValue
        {
            S = message.Content
        },
        ["contentHash"] = new AttributeValue
        {
            S = message.ContentHash
        },
        ["createdAt"] = new AttributeValue
        {
            S = message.CreatedAt.ToString("O", CultureInfo.InvariantCulture)
        }
    };

    public static Message FromMap(Dictionary<string, AttributeValue> map) => new()
    {
        Id = Guid.Parse(map["id"].S),
        Topic = map["topic"].S,
        EventName = map["eventName"].S,
        Content = map["content"].S,
        ContentHash = map["contentHash"].S,
        CreatedAt = DateTime.Parse(map["createdAt"].S, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
    };
}