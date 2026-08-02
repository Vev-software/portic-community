namespace Portic.Core.Contracts;

/// <summary>
/// Normalized inbound request to the gateway. This is the stable, provider-neutral shape accepted by
/// <c>POST /v1/messages</c>; provider adapters map it onto their SDK/wire types.
/// </summary>
public sealed record ChatRequest
{
    /// <summary>Logical model identifier, e.g. "stub-echo". The router/adapter maps it to a concrete model.</summary>
    public required string Model { get; init; }

    /// <summary>Ordered conversation history. Must contain at least one message.</summary>
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    /// <summary>Optional soft cap on generated tokens. Adapters honor it best-effort.</summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Optional explicit provider name (e.g. "stub"). When null/blank the router selects the
    /// configured default provider. Kept separate from <see cref="Model"/> so routing policy is not
    /// smuggled into the model string.
    /// </summary>
    public string? Provider { get; init; }
}
