using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Acme.Domain.Events;

public sealed class DomainEvent : IDomainEvent
{
    private const string DefaultTopic = "default";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public required Guid Id { get; init; }
    public required string EventName { get; init; }
    public required string Content { get; init; }
    public required string ContentHash { get; init; }
    public required string Topic { get; init; }
    public required DateTime CreatedAt { get; init; }

    public T ToT<T>() => JsonSerializer.Deserialize<T>(Content, JsonSerializerOptions)!;

    public override string ToString() =>
        $"{{{nameof(Id)}={Id}, {nameof(EventName)}={EventName}, {nameof(Topic)}={Topic}}}";

    public static DomainEvent Create<TPayload>(string eventName, TPayload payload, string? topic = null)
    {
        var jsonContent = JsonSerializer.Serialize(payload, JsonSerializerOptions);
        var jsonContentHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(jsonContent)));

        return new DomainEvent
        {
            Id = Guid.NewGuid(),
            EventName = eventName,
            Content = jsonContent,
            ContentHash = jsonContentHash,
            Topic = topic ?? DefaultTopic,
            CreatedAt = DateTime.UtcNow
        };
    }
}