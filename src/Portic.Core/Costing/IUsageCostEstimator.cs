namespace Portic.Core.Costing;

/// <summary>
/// Provider-neutral port for estimating the monetary cost of one completed gateway call.
///
/// Estimates are operational guidance only: they are not invoice truth and must later be reconciled
/// against provider billing. Community ships a default "unknown pricing" implementation so callers
/// can see explicitly when no estimate is available instead of silently treating the cost as zero.
/// </summary>
public interface IUsageCostEstimator
{
    UsageCostEstimate Estimate(UsageCostEstimationInput input);
}
