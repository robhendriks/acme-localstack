using Acme.Framework;
using Acme.Framework.Configuration;
using Acme.Infrastructure.Events;
using Acme.Persistence.Common.Storage;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
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
            .AddAcmeOutbox();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
#if DEBUG
        using var cts = new CancellationTokenSource();
#else
        using var cts = new CancellationTokenSource(context.RemainingTime);
#endif

        var db = _serviceProvider.GetRequiredService<IAmazonDb>();
        var outbox = _serviceProvider.GetRequiredService<ITransactionalOutbox>();

        outbox.Publish("Foo", new { Foo = "Bar", Baz = 1337 });
        outbox.Publish("Bar", new { Boo = "Bar", Baz = 1337 });

        await db.SaveChangesAsync(cts.Token);

        return new APIGatewayProxyResponse { StatusCode = 200 };
    }
}