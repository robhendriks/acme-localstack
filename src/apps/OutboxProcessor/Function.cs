using System.Globalization;
using System.Net;
using System.Text.Json;
using Acme.Domain.InboxOutbox;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.OutboxProcessor;

public class Function
{
    private readonly AmazonSimpleNotificationServiceClient _sns = new();

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var messages = CreateMessages(sqsEvent.Records, context);

        var snsTopicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")
                          ?? throw new InvalidOperationException("Environment variable 'SNS_TOPIC_ARN' not set.");

        var request = new PublishBatchRequest
        {
            TopicArn = snsTopicArn,
            PublishBatchRequestEntries = messages.ConvertAll(m => new PublishBatchRequestEntry
            {
                Id = m.Id.ToString("D"),
                Message = JsonSerializer.Serialize(m, Message.JsonSerializerOptions),
                MessageDeduplicationId = m.Id.ToString("D"),
            })
        };

        var snsResponse = await _sns.PublishBatchAsync(request);

        if (snsResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Failed to publish messages.");
        }

        // TODO: DELETE outbox message or set TTL
    }

    private static List<Message> CreateMessages(
        List<SQSEvent.SQSMessage> sqsEventRecords, ILambdaContext context) =>
        sqsEventRecords
            .ConvertAll(
                m =>
                {
                    context.Logger.LogInformation($"Processing message: {m.Body}");

                    var dynamodbStreamRecord =
                        JsonSerializer.Deserialize<DynamoDbEvent>(m.Body, Message.JsonSerializerOptions)!;
                    var map = dynamodbStreamRecord.DynamoDb.NewImage;

                    return new Message
                    {
                        Id = Guid.Parse(map["id"].S),
                        Topic = map["topic"].S,
                        EventName = map["eventName"].S,
                        Content = map["content"].S,
                        ContentHash = map["contentHash"].S,
                        CreatedAt = DateTime.Parse(map["createdAt"].S, CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal)
                    };
                });
}

public class DynamoDbEventStream
{
    public Dictionary<string, AttributeValue> NewImage { get; set; } = null!;
}

public class DynamoDbEvent
{
    public DynamoDbEventStream DynamoDb { get; set; } = null!;
}