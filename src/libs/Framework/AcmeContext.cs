namespace Acme.Framework;

public sealed record AcmeContext(string Application)
{
    public static AcmeContext FromEnvironment()
        => new(
            GetRequiredEnvironmentVariable("ACME_APPLICATION")
        );

    private static string GetRequiredEnvironmentVariable(string name)
        => Environment.GetEnvironmentVariable(name) ??
           throw new InvalidOperationException($"Environment variable '{name}' was not set.");
}