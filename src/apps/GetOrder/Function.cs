using Acme.Application;
using Acme.Application.Orders.Commands;
using Acme.Application.Orders.Queries;
using Acme.Framework;
using Acme.Framework.Results;
using Acme.Infrastructure.Events.Outbox;
using Acme.Infrastructure.Orders;
using Acme.Persistence.Common.Storage;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.GetOrder;

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

        var orderId = Guid.Parse(request.PathParameters["orderId"]);

        var sender = _serviceProvider.GetRequiredService<ISender>();
        var result = await sender.Send(new GetOrderQuery(orderId), cts.Token);

        return result.ToApiGatewayProxyResponse();
    }
}