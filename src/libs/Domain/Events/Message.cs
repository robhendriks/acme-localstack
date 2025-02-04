﻿using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Acme.Domain.Events;

public sealed class Message
{
    private const string DefaultTopic = "default";

    private static JsonSerializerOptions _jsonSerializerOptions = new()
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

    public static Message Create<TPayload>(string eventName, TPayload payload, string? topic = null)
    {
        var jsonContent = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var jsonContentHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(jsonContent)));

        return new Message
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