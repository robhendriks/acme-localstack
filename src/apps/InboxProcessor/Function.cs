using System.Text.Json;
using Acme.Domain.InboxOutbox;
using Acme.Infrastructure.Storage;
using Acme.Persistence.InboxOutbox;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.InboxProcessor;

public class Function
{
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        using var cts = new CancellationTokenSource(context.RemainingTime);

        using var client = new AmazonDynamoDBClient();
        var db = new AmazonDatabase(client);

        var tableName = Environment.GetEnvironmentVariable("INBOX_TABLE_NAME")
                        ?? throw new InvalidOperationException("Environment variable 'INBOX_TABLE_NAME' not set.");

        foreach (var sqsMessage in sqsEvent.Records)
        {
            context.Logger.LogInformation($"Processing message {sqsMessage.Body}");
            var message = JsonSerializer.Deserialize<Message>(sqsMessage.Body)!;

            db.Put(new PutItemRequest
            {
                TableName = tableName,
                Item = MessageMapper.ToMap(message)
            });
        }

        var result = await db.SaveChangesAsync(cts.Token);
        if (result.IsFailed)
        {
            throw new InvalidOperationException(result.Errors[0].Message);
        }
    }
}