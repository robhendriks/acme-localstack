using Acme.Infrastructure.Events;
using Acme.Infrastructure.Events.Inbox;
using Acme.Persistence.Common.Storage;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Framework.Events;

public sealed record DomainEventHandlerContext<TContent>(TContent Content, ILambdaContext LambdaContext);

public interface IDomainEventHandler<TContent>
{
    Task<Result> HandleAsync(DomainEventHandlerContext<TContent> context,
        CancellationToken cancellationToken = default);
}

public abstract class DomainEventHandler<TContent> : IDomainEventHandler<TContent>
{
    private readonly IServiceProvider _serviceProvider;

    protected DomainEventHandler()
    {
        var ctx = AcmeContext.FromEnvironment();

        var services = new ServiceCollection();

        services
            .AddAcmeFramework(ctx)
            .AddAcmeStorage()
            .AddAcmeInbox();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext lambdaContext)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(lambdaContext.RemainingTime);
#endif

        var inbox = _serviceProvider.GetRequiredService<ITransactionalInbox>();
        var amazonDb = _serviceProvider.GetRequiredService<IAmazonDb>();

        var response = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var domainEvent = DomainEventSerializer.DeserializeDynamoDbStreamEvent(record.Body);

                var content = domainEvent.ToT<TContent>();
                var context = new DomainEventHandlerContext<TContent>(content, lambdaContext);

                var result = await HandleAsync(context, cts.Token);
                if (result.IsFailed)
                {
                    throw new InvalidOperationException("Failed to handle domain event");
                }

                inbox.Consume(domainEvent);

                var saveResult = await amazonDb.SaveChangesAsync(cts.Token);
                if (saveResult.IsFailed)
                {
                    throw new InvalidOperationException("Failed to commit DynamoDb transaction");
                }
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