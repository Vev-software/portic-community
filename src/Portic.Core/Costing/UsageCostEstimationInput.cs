namespace Portic.Core.Costing;

public sealed record UsageCostEstimationInput
{
    public required string Provider { get; init; }

    public required string Model { get; init; }

    public required int InputTokens { get; init; }

    public required int OutputTokens { get; init; }
}
