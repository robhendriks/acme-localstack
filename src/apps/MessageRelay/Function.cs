using System.Net;
using System.Text.Json;
using Acme.Domain.Events;
using Acme.Infrastructure.Events;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.MessageRelay;

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

        var amazonDb = new AmazonDynamoDBClient();
        var amazonDbTableName = Environment.GetEnvironmentVariable("OUTBOX_TABLE_NAME");

        var amazonSns = new AmazonSimpleNotificationServiceClient();
        var amazonSnsTopicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")!;

        var response = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                context.Logger.LogInformation(
                    $"Relay SQS message '{record.MessageId}' to SNS topic '{amazonSnsTopicArn}'"
                );

                var domainEvent = CreateDomainEvent(record);

                await RelaySqsMessageToSns(
                    amazonSns,
                    amazonSnsTopicArn,
                    domainEvent,
                    cts.Token
                );

                await DeleteFromOutbox(
                    domainEvent,
                    amazonDb,
                    amazonDbTableName,
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

    private static IDomainEvent CreateDomainEvent(SQSEvent.SQSMessage sqsMessage)
    {
        var dynamoDbEvent = JsonSerializer.Deserialize<DynamoDbEvent>(sqsMessage.Body, JsonSerializerOptions)!;
        return DomainEventMapper.FromMap(dynamoDbEvent.DynamoDb.NewImage);
    }

    private static async Task RelaySqsMessageToSns(
        AmazonSimpleNotificationServiceClient amazonSns,
        string amazonSnsTopicArn,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var domainEventJson = JsonSerializer.Serialize(domainEvent, JsonSerializerOptions);

        var result = await amazonSns.PublishAsync(
            amazonSnsTopicArn,
            domainEventJson,
            cancellationToken
        );

        if (result.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to publish SNS message to topic '{amazonSnsTopicArn}'.");
        }
    }

    private static Task DeleteFromOutbox(
        IDomainEvent domainEvent,
        AmazonDynamoDBClient cancellationToken,
        string? amazonDbTableName,
        CancellationToken ctsToken)
    {
        // TODO: Implement

        return Task.CompletedTask;
    }
}

file sealed record DynamoDbEventRecord
{
    public required Dictionary<string, AttributeValue> NewImage { get; init; }
}

file sealed record DynamoDbEvent
{
    public required DynamoDbEventRecord DynamoDb { get; init; }
}