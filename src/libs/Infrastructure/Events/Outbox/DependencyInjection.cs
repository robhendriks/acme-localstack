using Microsoft.Extensions.DependencyInjection;

namespace Acme.Infrastructure.Events.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeOutbox(this IServiceCollection services)
    {
        services
            .AddOptionsWithValidateOnStart<OutboxOptions>()
            .BindConfiguration("Outbox");

        services.AddSingleton<ITransactionalOutbox, TransactionalOutbox>();

        return services;
    }
}