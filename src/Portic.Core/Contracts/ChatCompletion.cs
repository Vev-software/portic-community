namespace Portic.Core.Contracts;

/// <summary>
/// Normalized outbound response from the gateway. Returned by <c>POST /v1/messages</c> regardless of
/// which provider served the request, so callers are insulated from provider-specific response shapes.
/// </summary>
public sealed record ChatCompletion
{
    /// <summary>Gateway-assigned completion id.</summary>
    public required string Id { get; init; }

    /// <summary>Logical model that produced the completion.</summary>
    public required string Model { get; init; }

    /// <summary>Name of the provider adapter that served the request.</summary>
    public required string Provider { get; init; }

    /// <summary>The assistant message.</summary>
    public required ChatMessage Message { get; init; }

    /// <summary>Token accounting for cost/telemetry.</summary>
    public required TokenUsage Usage { get; init; }
}
