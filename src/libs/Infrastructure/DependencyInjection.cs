using Acme.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        _ = services.AddStorageServices();

        return services;
    }
}