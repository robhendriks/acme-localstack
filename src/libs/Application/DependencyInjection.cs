using System.Reflection;
using Acme.Application.Common.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddCqrsServices(assemblies);

        return services;
    }
}