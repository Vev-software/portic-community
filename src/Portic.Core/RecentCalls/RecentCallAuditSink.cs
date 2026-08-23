using Portic.Core.Observability;

namespace Portic.Core.RecentCalls;

/// <summary>
/// Read-side projection from write-side audit events into the Community recent-call view.
/// </summary>
public sealed class RecentCallAuditSink(IRecentCallStore store) : IAuditSink
{
    public Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Append(RecentCallRecord.FromAuditEvent(auditEvent));
        return Task.CompletedTask;
    }
}
