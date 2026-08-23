using Portic.Core.Costing;

namespace Portic.Core.Observability;

public enum AuditIdentityState
{
    Placeholder,
    External,
}

public enum AuditContentState
{
    Withheld,
    Sanitized,
    Stored,
}

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

    /// <summary>Logical gateway route, currently the HTTP slice name (e.g. "POST /v1/messages").</summary>
    public required string Route { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }

    /// <summary>"success" or "error".</summary>
    public required string Outcome { get; init; }

    /// <summary>Wall-clock duration of the gateway operation in whole milliseconds.</summary>
    public required long LatencyMs { get; init; }

    /// <summary>Community today emits placeholder identity; richer Fabric-backed identity can replace it later.</summary>
    public required AuditIdentityState IdentityState { get; init; }

    public required string TenantId { get; init; }

    public required string PrincipalId { get; init; }

    /// <summary>Whether request content was withheld, sanitized, or stored in audit metadata.</summary>
    public required AuditContentState RequestContentState { get; init; }

    /// <summary>Whether response content was withheld, sanitized, or stored in audit metadata.</summary>
    public required AuditContentState ResponseContentState { get; init; }

    public int? InputTokens { get; init; }

    public int? OutputTokens { get; init; }

    /// <summary>Estimate status; never infer "zero cost" from a missing estimate.</summary>
    public required AuditCostEstimationStatus CostEstimationStatus { get; init; }

    public decimal? EstimatedCost { get; init; }

    public string? EstimatedCostCurrency { get; init; }

    public required string CostEstimationSource { get; init; }

    /// <summary>Machine-readable reason code on failure (e.g. "provider_not_found"), else null.</summary>
    public string? ReasonCode { get; init; }
}
