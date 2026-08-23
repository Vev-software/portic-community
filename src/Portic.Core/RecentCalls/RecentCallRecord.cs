using Portic.Core.Costing;
using Portic.Core.Observability;

namespace Portic.Core.RecentCalls;

/// <summary>
/// Community read model for recent gateway calls. This is intentionally a bounded, in-process view and
/// not a durable audit database.
/// </summary>
public sealed record RecentCallRecord
{
    public required string EventType { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string Route { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }

    public required string Outcome { get; init; }

    public required long LatencyMs { get; init; }

    public required AuditIdentityState IdentityState { get; init; }

    public required string TenantId { get; init; }

    public required string PrincipalId { get; init; }

    public required AuditContentState RequestContentState { get; init; }

    public required AuditContentState ResponseContentState { get; init; }

    public int? InputTokens { get; init; }

    public int? OutputTokens { get; init; }

    public required AuditCostEstimationStatus CostEstimationStatus { get; init; }

    public decimal? EstimatedCost { get; init; }

    public string? EstimatedCostCurrency { get; init; }

    public required string CostEstimationSource { get; init; }

    public string? ReasonCode { get; init; }

    public static RecentCallRecord FromAuditEvent(AuditEvent auditEvent)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);

        return new RecentCallRecord
        {
            EventType = auditEvent.EventType,
            Timestamp = auditEvent.Timestamp,
            Route = auditEvent.Route,
            Provider = auditEvent.Provider,
            Model = auditEvent.Model,
            Outcome = auditEvent.Outcome,
            LatencyMs = auditEvent.LatencyMs,
            IdentityState = auditEvent.IdentityState,
            TenantId = auditEvent.TenantId,
            PrincipalId = auditEvent.PrincipalId,
            RequestContentState = auditEvent.RequestContentState,
            ResponseContentState = auditEvent.ResponseContentState,
            InputTokens = auditEvent.InputTokens,
            OutputTokens = auditEvent.OutputTokens,
            CostEstimationStatus = auditEvent.CostEstimationStatus,
            EstimatedCost = auditEvent.EstimatedCost,
            EstimatedCostCurrency = auditEvent.EstimatedCostCurrency,
            CostEstimationSource = auditEvent.CostEstimationSource,
            ReasonCode = auditEvent.ReasonCode,
        };
    }
}
