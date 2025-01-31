using Microsoft.Extensions.Configuration;

namespace Acme.Persistence;

public static class Configuration
{
    public static IConfigurationManager AddPersistenceConfiguration(this IConfigurationManager configuration)
    {
        configuration
            .AddSystemsManager(cfg =>
            {
                cfg.Prefix = "OrderTable";
                cfg.Path = "/OrderTable/";
                cfg.ReloadAfter = TimeSpan.FromMinutes(5);
            });

        configuration
            .AddSystemsManager(cfg =>
            {
                cfg.Prefix = "OutboxTable";
                cfg.Path = "/OutboxTable/";
                cfg.ReloadAfter = TimeSpan.FromMinutes(5);
            });

        return configuration;
    }
}