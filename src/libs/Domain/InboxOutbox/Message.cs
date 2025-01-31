using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Acme.Domain.InboxOutbox;

public sealed record Message
{
    private const string DefaultTopic = "default";

    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public required Guid Id { get; init; }
    public required string Topic { get; init; }
    public required string EventName { get; init; }
    public required string Content { get; init; }
    public required string ContentHash { get; init; }
    public required DateTime CreatedAt { get; init; }

    public static Message Create<TContent>(TContent content, string eventName, string? topic = null)
    {
        var jsonContent = JsonSerializer.Serialize(content, JsonSerializerOptions);
        var jsonContentHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(jsonContent)));

        return new Message
        {
            Id = Guid.NewGuid(),
            Topic = topic ?? DefaultTopic,
            EventName = eventName,
            Content = jsonContent,
            ContentHash = jsonContentHash,
            CreatedAt = DateTime.UtcNow
        };
    }
}