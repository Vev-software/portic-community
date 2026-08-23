namespace Portic.Core.RecentCalls;

public sealed class RecentCallQueryService(IRecentCallStore store) : IRecentCallQueryService
{
    public IReadOnlyList<RecentCallRecord> QueryRecentCalls(RecentCallQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return store.Query(query);
    }
}
