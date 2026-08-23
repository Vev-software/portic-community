namespace Portic.Core.Costing;

public enum AuditCostEstimationStatus
{
    NotComputed,
    UnknownPricing,
    Estimated,
}

public sealed record UsageCostEstimate
{
    public required AuditCostEstimationStatus Status { get; init; }

    public decimal? Amount { get; init; }

    public string? Currency { get; init; }

    public required string Source { get; init; }
}
