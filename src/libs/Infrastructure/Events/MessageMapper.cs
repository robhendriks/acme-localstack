using Acme.Domain.Events;
using Amazon.DynamoDBv2.Model;

namespace Acme.Infrastructure.Events;

internal static class MessageMapper
{
    public static Dictionary<string, AttributeValue> ToMap(Message message) => new()
    {
        ["id"] = new AttributeValue
        {
            S = message.Id.ToString("D")
        },
        ["eventName"] = new AttributeValue
        {
            S = message.EventName
        },
        ["topic"] = new AttributeValue
        {
            S = message.Topic
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
            S = message.CreatedAt.ToString("O")
        }
    };
}