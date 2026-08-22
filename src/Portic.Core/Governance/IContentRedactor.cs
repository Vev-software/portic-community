namespace Portic.Core.Governance;

/// <summary>
/// Sanitizes prompt/completion content before it could reach any persisted or displayed log. This is
/// a thin port ahead of the feature that will consume it (the planned usage/audit view,
/// portic-community#17), the same pattern as <c>IAuditSink</c> in ADR-0002: today's <see
/// cref="Portic.Core.Observability.AuditEvent"/> already carries no content field at all, so there is
/// no current call site that needs redacted text -- this port exists so that when a content-aware log
/// lands, redaction is a mandatory step in front of it from day one, never an afterthought.
/// </summary>
public interface IContentRedactor
{
    /// <summary>Return <paramref name="content"/> with recognizable PII replaced by a redaction marker.</summary>
    string Redact(string content);
}
