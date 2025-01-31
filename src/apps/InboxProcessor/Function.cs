using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.InboxProcessor;

public class Function
{
    public Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var message in sqsEvent.Records)
        {
            context.Logger.LogInformation($"Processing message {message.Body}");
        }

        return Task.CompletedTask;
    }
}