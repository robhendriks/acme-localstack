using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Application.Common.Cqrs;

internal static class DependencyInjection
{
    internal static IServiceCollection AddCqrsServices(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblies(assemblies); });

        return services;
    }
}