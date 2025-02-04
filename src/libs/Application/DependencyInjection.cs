using Acme.Application.Common.Transactions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IApplicationLayer>();
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionalBehavior<,>));
        });

        return services;
    }
}