namespace Portic.Core.Contracts;

/// <summary>
/// Normalized token accounting. Adapters populate this from provider usage metadata (or estimate it
/// for local stubs). Used for cost/telemetry — it never carries prompt or completion content.
/// </summary>
public sealed record TokenUsage
{
    public required int InputTokens { get; init; }

    public required int OutputTokens { get; init; }

    public int TotalTokens => InputTokens + OutputTokens;
}
