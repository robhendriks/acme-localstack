using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Infrastructure.Storage;

internal static class DependencyInjection
{
    internal static IServiceCollection AddStorageServices(this IServiceCollection services)
    {
        _ = services.AddSingleton(CreateAmazonDatabase);

        return services;
    }

    private static IAmazonDatabase CreateAmazonDatabase(IServiceProvider provider)
    {
        var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName("eu-west-1")
        });

        return new AmazonDatabase(client);
    }
}