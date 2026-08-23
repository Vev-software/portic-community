namespace Portic.Core.RecentCalls;

public interface IRecentCallQueryService
{
    IReadOnlyList<RecentCallRecord> QueryRecentCalls(RecentCallQuery query);
}
