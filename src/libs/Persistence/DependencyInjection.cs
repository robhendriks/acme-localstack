using Acme.Persistence.InboxOutbox;
using Acme.Persistence.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services
            .AddOptions<OrderTableOptions>()
            .BindConfiguration("OrderTable")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddSingleton<IOrderRepository, OrderRepository>()
            .AddSingleton<IOutboxRepository, OutboxRepository>();

        return services;
    }
}