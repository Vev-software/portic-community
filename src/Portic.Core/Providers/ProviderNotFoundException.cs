namespace Portic.Core.Providers;

/// <summary>
/// Thrown when a request targets a provider name that no registered adapter serves. The gateway maps
/// this to an HTTP 400 with a reason code — it is a caller error, not a server fault.
/// </summary>
public sealed class ProviderNotFoundException(string providerName)
    : Exception($"No provider adapter is registered for '{providerName}'.")
{
    public string ProviderName { get; } = providerName;
}
