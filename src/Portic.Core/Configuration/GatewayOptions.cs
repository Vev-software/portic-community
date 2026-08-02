namespace Portic.Core.Configuration;

/// <summary>
/// Gateway configuration, bound from the "Portic" configuration section (environment variables /
/// appsettings). Provider <b>credentials are never modeled here</b>; adapters read their own secrets
/// straight from the environment so secrets never flow through shared config objects or telemetry.
/// </summary>
public sealed class GatewayOptions
{
    public const string SectionName = "Portic";

    /// <summary>Provider used when a request does not name one explicitly.</summary>
    public string DefaultProvider { get; set; } = "stub";
}
