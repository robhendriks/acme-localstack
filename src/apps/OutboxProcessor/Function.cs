using System.Globalization;
using System.Net;
using System.Text.Json;
using Acme.Domain.Events;
using Acme.Framework;
using Acme.Infrastructure.Events;
using Acme.Infrastructure.Events.Outbox;
using Acme.Persistence.Common.Storage;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.OutboxProcessor;

public sealed class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        var ctx = AcmeContext.FromEnvironment();

        var services = new ServiceCollection();

        services
            .AddAcmeFramework(ctx)
            .AddAcmeStorage()
            .AddAcmeOutbox();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(context.RemainingTime);
#endif

        var outbox = _serviceProvider.GetRequiredService<ITransactionalOutbox>();
        var amazonDb = _serviceProvider.GetRequiredService<IAmazonDb>();

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

                var domainEvent = DomainEventSerializer.DeserializeDynamoDbStreamEvent(record.Body);

                await RelaySqsMessageToSns(
                    amazonSns,
                    amazonSnsTopicArn,
                    domainEvent,
                    cts.Token
                );

                outbox.Consume(domainEvent);

                await amazonDb.SaveChangesOrThrowAsync(cts.Token);
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

    private static async Task RelaySqsMessageToSns(
        AmazonSimpleNotificationServiceClient amazonSns,
        string amazonSnsTopicArn,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var domainEventJson = DomainEventSerializer.Serialize(domainEvent);

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
}