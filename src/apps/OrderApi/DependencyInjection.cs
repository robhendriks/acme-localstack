using Acme.Application;
using Acme.Infrastructure;
using Acme.Persistence;

namespace Acme.OrderApi;

internal static class DependencyInjection
{
    internal static IServiceCollection AddOrderApiServices(this IServiceCollection services)
    {
        var domainAssemblies = AppDomain
            .CurrentDomain
            .GetAssemblies();

        var applicationAssembly = typeof(IApplicationLayerMarker).Assembly;

        var assemblies = domainAssemblies
            .Append(applicationAssembly)
            .ToArray();


        services
            .AddInfrastructureServices()
            .AddPersistenceServices()
            .AddApplicationServices(assemblies);

        return services;
    }
}