using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acme.Persistence.Common.Storage;

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeStorage(this IServiceCollection services)
    {
        services.AddSingleton(AmazonDbFactory);

        return services;
    }

    private static IAmazonDb AmazonDbFactory(IServiceProvider serviceProvider)
    {
        var dynamoDbConfig = new AmazonDynamoDBConfig();

        return new AmazonDb(
            new AmazonDynamoDBClient(dynamoDbConfig),
            serviceProvider.GetRequiredService<ILogger<AmazonDb>>()
        );
    }
}