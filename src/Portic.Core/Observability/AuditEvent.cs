namespace Portic.Core.Observability;

/// <summary>
/// A structured, content-free record of one gateway operation, suitable for an audit trail.
///
/// By design this type has no field that can hold prompt or completion text — auditability must never
/// become a content-exfiltration path (AGENTS.md: "no customer content logged by default").
/// </summary>
public sealed record AuditEvent
{
    /// <summary>Dotted event type, e.g. "ai.message.completed".</summary>
    public required string EventType { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }

    /// <summary>"success" or "error".</summary>
    public required string Outcome { get; init; }

    public int? InputTokens { get; init; }

    public int? OutputTokens { get; init; }

    /// <summary>Machine-readable reason code on failure (e.g. "provider_not_found"), else null.</summary>
    public string? ReasonCode { get; init; }
}
