using Microsoft.Extensions.Logging;

namespace Portic.Core.Observability;

/// <summary>
/// Minimal community-edition <see cref="IAuditSink"/> that writes structured, content-free audit
/// records through <see cref="ILogger"/>. Placeholder pending the Fabric audit contract (ADR-0002):
/// it deliberately does no persistence, batching, or tamper-evidence — those belong to Fabric.
/// </summary>
public sealed partial class LoggingAuditSink(ILogger<LoggingAuditSink> logger) : IAuditSink
{
    public Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        Audit(
            logger,
            auditEvent.EventType,
            auditEvent.Provider,
            auditEvent.Model,
            auditEvent.Outcome,
            auditEvent.ReasonCode,
            auditEvent.InputTokens,
            auditEvent.OutputTokens);
        return Task.CompletedTask;
    }

    // Source-generated log message: no message content, only routing/cost/outcome metadata.
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "audit {EventType} provider={Provider} model={Model} outcome={Outcome} reason={ReasonCode} tokensIn={InputTokens} tokensOut={OutputTokens}")]
    private static partial void Audit(
        ILogger logger,
        string eventType,
        string provider,
        string model,
        string outcome,
        string? reasonCode,
        int? inputTokens,
        int? outputTokens);
}
