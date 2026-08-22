namespace Portic.Core.Governance;

/// <summary>
/// Thrown when governance policy denies a request (model not allowlisted, team quota exhausted).
/// The gateway maps this to a reason-coded HTTP response -- a caller/policy condition, not a server
/// fault (mirrors <see cref="Portic.Core.Providers.ProviderNotFoundException"/>).
/// </summary>
public sealed class PolicyDeniedException(string reasonCode, string message) : Exception(message)
{
    public string ReasonCode { get; } = reasonCode;
}
