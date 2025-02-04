using Acme.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Acme.Framework;

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeFramework(
        this IServiceCollection services,
        AcmeContext context)
    {
        var configuration = new ConfigurationBuilder();
        configuration.AddAcmeConfiguration(context);

        services
            .AddSingleton<IConfiguration>(configuration.Build())
            .AddLogging()
            .AddSerilog();

        return services;
    }
}