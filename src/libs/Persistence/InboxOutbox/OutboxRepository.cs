using System.Text.Json;
using Acme.Infrastructure.Storage;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace Acme.Persistence.InboxOutbox;

internal sealed class OutboxRepository(
    IAmazonDatabase database,
    IOptions<OutboxTableOptions> options)
    : IOutboxRepository
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Create<TMessage>(string eventName, TMessage message)
    {
        database.Put(new PutItemRequest
        {
            TableName = options.Value.TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["id"] = new()
                {
                    S = Guid.NewGuid().ToString("D")
                },
                ["name"] = new()
                {
                    S = eventName
                },
                ["messageType"] = new()
                {
                    S = typeof(TMessage).FullName!
                },
                ["messageBody"] = new()
                {
                    S = JsonSerializer.Serialize(message, JsonSerializerOptions)
                }
            }
        });
    }
}