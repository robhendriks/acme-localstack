using Acme.Infrastructure.Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FluentResults;

namespace Acme.Framework.Events;

public sealed record DomainEventHandlerContext<TContent>(TContent Content, ILambdaContext LambdaContext);

public interface IDomainEventHandler<TContent>
{
    Task<Result> HandleAsync(DomainEventHandlerContext<TContent> context,
        CancellationToken cancellationToken = default);
}

public abstract class DomainEventHandler<TContent> : IDomainEventHandler<TContent>
{
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext lambdaContext)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(lambdaContext.RemainingTime);
#endif

        var response = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var domainEvent = DomainEventSerializer.DeserializeDynamoDbStreamEvent(record.Body);

                var content = domainEvent.ToT<TContent>();
                var context = new DomainEventHandlerContext<TContent>(content, lambdaContext);

                await HandleAsync(context, cts.Token);

                // TODO: remove from inbox
            }
            catch (Exception ex)
            {
                lambdaContext.Logger.LogError(ex, "Error while processing SQS message.");

                response.BatchItemFailures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = record.MessageId
                });
            }
        }

        return response;
    }

    public abstract Task<Result> HandleAsync(
        DomainEventHandlerContext<TContent> content,
        CancellationToken cancellationToken = default
    );
}