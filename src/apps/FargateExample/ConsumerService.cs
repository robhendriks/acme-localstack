using System.Net;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Acme.FargateExample;

public sealed class ConsumerService(ILogger<ConsumerService> logger, IAmazonSQS sqs) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL")!;

        var messageRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MessageAttributeNames = ["All"],
            MessageSystemAttributeNames = ["All"]
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var messageResponse = await sqs.ReceiveMessageAsync(messageRequest, stoppingToken);

            if (messageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                logger.LogWarning("SQS returned status code {StatusCode}", messageResponse.HttpStatusCode);
                continue;
            }

            foreach (var message in messageResponse.Messages)
            {
                logger.LogInformation("Received message: {MessageBody}", message.Body);
                await sqs.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting consumer service.");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping consumer service.");
        return base.StopAsync(cancellationToken);
    }
}