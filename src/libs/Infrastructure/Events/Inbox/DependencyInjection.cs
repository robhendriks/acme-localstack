using Microsoft.Extensions.DependencyInjection;

namespace Acme.Infrastructure.Events.Inbox;

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeInbox(this IServiceCollection services)
    {
        services
            .AddOptionsWithValidateOnStart<InboxOptions>()
            .BindConfiguration("Inbox");

        services.AddSingleton<ITransactionalInbox, TransactionalInbox>();

        return services;
    }
}