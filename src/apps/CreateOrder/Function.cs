using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Acme.CreateOrder;

public sealed class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        var services = new ServiceCollection();

        _serviceProvider = services.BuildServiceProvider();
    }

    public Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Hello World!");

        return Task.FromResult(new APIGatewayProxyResponse { StatusCode = 200 });
    }
}