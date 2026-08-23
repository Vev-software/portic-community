namespace Portic.Core.Costing;

/// <summary>
/// Community default: emit an explicit unknown-pricing state until a host composes in a real pricing
/// source. This keeps the gateway honest about what it knows and prevents "0 cost" from being read as
/// a real billing figure.
/// </summary>
public sealed class UnknownUsageCostEstimator : IUsageCostEstimator
{
    public const string DefaultSource = "community-default-unknown-pricing";

    public UsageCostEstimate Estimate(UsageCostEstimationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new UsageCostEstimate
        {
            Status = AuditCostEstimationStatus.UnknownPricing,
            Amount = null,
            Currency = null,
            Source = DefaultSource,
        };
    }
}
