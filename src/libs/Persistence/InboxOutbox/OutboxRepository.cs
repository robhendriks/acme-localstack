using System.Security.Cryptography;
using System.Text;
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
        var content = JsonSerializer.Serialize(message, JsonSerializerOptions);
        var contentHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        database.Put(new PutItemRequest
        {
            TableName = options.Value.TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["id"] = new()
                {
                    S = Guid.NewGuid().ToString("D")
                },
                ["eventName"] = new()
                {
                    S = eventName
                },
                ["content"] = new()
                {
                    S = content
                },
                ["contentHash"] = new()
                {
                    S = contentHash
                },
                ["createdAt"] = new()
                {
                    S = DateTime.UtcNow.ToString("O")
                }
            }
        });
    }
}