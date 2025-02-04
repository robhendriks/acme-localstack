using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.InboxProcessor;

public sealed class Function
{
    public Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(context.RemainingTime);
#endif

        var response = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            context.Logger.LogInformation($"Processing record {record.Body}");
        }

        return Task.FromResult(response);
    }
}