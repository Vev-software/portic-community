namespace Portic.Core.RecentCalls;

/// <summary>
/// Community-grade bounded store for recent call records. Capacity is fixed and oldest rows are
/// evicted in insertion order when the buffer is full.
/// </summary>
public sealed class BoundedRecentCallStore : IRecentCallStore
{
    public const int DefaultCapacity = 256;

    private readonly object gate = new();
    private readonly RecentCallRecord[] buffer;
    private int nextWriteIndex;
    private int count;

    public BoundedRecentCallStore(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");
        }

        buffer = new RecentCallRecord[capacity];
    }

    public void Append(RecentCallRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (gate)
        {
            buffer[nextWriteIndex] = record;
            nextWriteIndex = (nextWriteIndex + 1) % buffer.Length;

            if (count < buffer.Length)
            {
                count++;
            }
        }
    }

    public IReadOnlyList<RecentCallRecord> Query(RecentCallQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        RecentCallRecord[] snapshot;
        lock (gate)
        {
            snapshot = SnapshotUnsafe();
        }

        return snapshot
            .Where(record => Matches(record, query))
            .OrderByDescending(record => record.Timestamp)
            .ToArray();
    }

    private RecentCallRecord[] SnapshotUnsafe()
    {
        var snapshot = new RecentCallRecord[count];
        var start = count == buffer.Length ? nextWriteIndex : 0;

        for (var i = 0; i < count; i++)
        {
            snapshot[i] = buffer[(start + i) % buffer.Length];
        }

        return snapshot;
    }

    private static bool Matches(RecentCallRecord record, RecentCallQuery query)
    {
        if (query.Provider is not null && !string.Equals(record.Provider, query.Provider, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (query.Model is not null && !string.Equals(record.Model, query.Model, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (query.Outcome is not null && !string.Equals(record.Outcome, query.Outcome, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (query.Since is not null && record.Timestamp < query.Since.Value)
        {
            return false;
        }

        if (query.Until is not null && record.Timestamp > query.Until.Value)
        {
            return false;
        }

        return true;
    }
}
