namespace Portic.Core.Observability;

/// <summary>
/// Port for emitting <see cref="AuditEvent"/>s. Audit is a Fabric concern (AGENTS.md); Portic depends
/// on this port and ships only a minimal logging sink for the community edition. The real binding to
/// the Fabric audit contract is proposed in docs/adr/0002 — do NOT grow a local audit store here.
/// </summary>
public interface IAuditSink
{
    Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
