namespace Portic.Core.RecentCalls;

public interface IRecentCallStore
{
    void Append(RecentCallRecord record);

    IReadOnlyList<RecentCallRecord> Query(RecentCallQuery query);
}
