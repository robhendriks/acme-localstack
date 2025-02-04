using Microsoft.Extensions.DependencyInjection;

namespace Acme.Infrastructure.Orders;

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeOrders(this IServiceCollection services)
    {
        services
            .AddOptionsWithValidateOnStart<OrderOptions>()
            .BindConfiguration("Order");

        services.AddSingleton<IOrderRepo, OrderRepo>();

        return services;
    }
}