using System.Globalization;
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

namespace Acme.OutboxProcessor;

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

                await ProcessDomainEventInOutbox(
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
            throw new InvalidOperationException(
                $"Failed to publish SNS message to topic '{amazonSnsTopicArn}'."
            );
        }
    }

    private static async Task ProcessDomainEventInOutbox(
        IDomainEvent domainEvent,
        AmazonDynamoDBClient dynamoDb,
        string? dynamoDbTableName,
        CancellationToken cancellationToken)
    {
        var ttl = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

        var result = await dynamoDb.UpdateItemAsync(
            dynamoDbTableName,
            new Dictionary<string, AttributeValue>
            {
                ["id"] = new() { S = domainEvent.Id.ToString("D") }
            },
            new Dictionary<string, AttributeValueUpdate>
            {
                ["ttl"] = new()
                {
                    Action = AttributeAction.PUT,
                    Value = new AttributeValue { N = ttl.ToString(CultureInfo.InvariantCulture) }
                }
            },
            cancellationToken
        );

        if (result.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"Failed to update domain event '{domainEvent.Id}' in DynamoDB table '{dynamoDbTableName}'"
            );
        }
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