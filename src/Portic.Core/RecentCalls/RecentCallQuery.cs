namespace Portic.Core.RecentCalls;

public sealed record RecentCallQuery
{
    public string? Provider { get; init; }

    public string? Model { get; init; }

    public string? Outcome { get; init; }

    public DateTimeOffset? Since { get; init; }

    public DateTimeOffset? Until { get; init; }
}
