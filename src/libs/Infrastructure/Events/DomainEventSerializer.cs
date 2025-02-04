using System.Text.Json;
using Acme.Domain.Events;
using Amazon.DynamoDBv2.Model;

namespace Acme.Infrastructure.Events;

public static class DomainEventSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static IDomainEvent DeserializeDynamoDbStreamEvent(string content)
    {
        var dynamoDbEvent = JsonSerializer.Deserialize<DynamoDbStreamEvent>(content, JsonSerializerOptions)!;
        return DomainEventMapper.FromMap(dynamoDbEvent.DynamoDb.NewImage);
    }

    public static string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, JsonSerializerOptions);
}

file sealed record DynamoDbStreamEventRecord
{
    public required Dictionary<string, AttributeValue> NewImage { get; init; }
}

file sealed record DynamoDbStreamEvent
{
    public required DynamoDbStreamEventRecord DynamoDb { get; init; }
}