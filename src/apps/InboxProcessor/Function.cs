using System.Net;
using System.Text.Json;
using Acme.Domain.Events;
using Acme.Infrastructure.Events;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.InboxProcessor;

public sealed class Function
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(context.RemainingTime);
#endif

        var dynamoDb = new AmazonDynamoDBClient();
        var dynamoDbTableName = Environment.GetEnvironmentVariable("INBOX_TABLE_NAME")!;

        var response = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var domainEvent = CreateDomainEvent(record);

                context.Logger.LogInformation(
                    $"Processing domain event {domainEvent}."
                );

                await PutDomainEvent(
                    dynamoDb,
                    dynamoDbTableName,
                    domainEvent,
                    cts.Token
                );
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex, "Error while processing SQS message.");

                response.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId }
                );
            }
        }

        return response;
    }

    private static async Task PutDomainEvent(
        AmazonDynamoDBClient dynamoDb,
        string dynamoDbTableName,
        DomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var result = await dynamoDb.PutItemAsync(
            new PutItemRequest
            {
                TableName = dynamoDbTableName,
                Item = DomainEventMapper.ToMap(domainEvent)
            },
            cancellationToken
        );

        if (result.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"Failed to put domain event '{domainEvent.Id}' into DynamoDB table '{dynamoDbTableName}'"
            );
        }
    }

    private static DomainEvent CreateDomainEvent(SQSEvent.SQSMessage sqsMessage)
    {
        var snsMessage = JsonSerializer.Deserialize<SnsMessage>(sqsMessage.Body, JsonSerializerOptions)!;
        return JsonSerializer.Deserialize<DomainEvent>(snsMessage.Message, JsonSerializerOptions)!;
    }
}

file class SnsMessage
{
    public required string Message { get; init; }
}