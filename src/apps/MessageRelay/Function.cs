using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.MessageRelay;

public sealed class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        var services = new ServiceCollection();

        _serviceProvider = services.BuildServiceProvider();
    }

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
            context.Logger.LogInformation($"Record {record.Body}");
        }

        return Task.FromResult(response);
    }
}