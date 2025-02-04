using Microsoft.Extensions.Configuration;

namespace Acme.Framework.Configuration;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddAcmeConfiguration(this IConfigurationBuilder builder, AcmeContext context)
    {
        builder
            .AddSystemsManager(cfg =>
            {
                cfg.Path = $"/{context.Application}";
                cfg.Prefix = string.Empty;
                cfg.ReloadAfter = TimeSpan.FromMinutes(5);
            });

        return builder;
    }
}