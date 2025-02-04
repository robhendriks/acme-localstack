using Acme.Domain.Orders.Events;
using Acme.Framework.Events;
using Amazon.Lambda.Core;
using FluentResults;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.OrderRequestedProcessor;

public sealed class Function : DomainEventHandler<OrderRequested>
{
    public override Task<Result> HandleAsync(
        DomainEventHandlerContext<OrderRequested> context,
        CancellationToken cancellationToken = default)
    {
        context.LambdaContext.Logger.LogInformation($"Handling OrderRequested event {context.Content}");

        return Task.FromResult(Result.Ok());
    }
}