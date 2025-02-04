using System.Text.Json;
using Acme.Application;
using Acme.Application.Orders.Commands;
using Acme.Domain.Orders;
using Acme.Framework;
using Acme.Infrastructure.Events.Outbox;
using Acme.Infrastructure.Orders;
using Acme.Persistence.Common.Storage;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.CreateOrder;

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
            .AddAcmeOutbox()
            .AddAcmeOrders()
            .AddAcmeApplication();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(context.RemainingTime);
#endif

        var sender = _serviceProvider.GetRequiredService<ISender>();
        var result = await sender.Send(new CreateOrderCommand(), cts.Token);

        if (result.IsFailed)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 409
            };
        }

        return new APIGatewayProxyResponse { StatusCode = 201 };
    }
}