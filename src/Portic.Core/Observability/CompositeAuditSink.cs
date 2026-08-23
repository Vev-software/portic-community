namespace Portic.Core.Observability;

/// <summary>
/// Fan-out adapter so Community can keep structured audit logging while projecting a separate read
/// model for recent calls from the same write-side event.
/// </summary>
public sealed class CompositeAuditSink(IEnumerable<IAuditSink> sinks) : IAuditSink
{
    private readonly IAuditSink[] sinks = sinks.ToArray();

    public async Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        foreach (var sink in sinks)
        {
            await sink.RecordAsync(auditEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
