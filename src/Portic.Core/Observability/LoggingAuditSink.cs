using Microsoft.Extensions.Logging;
using Portic.Core.Costing;

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
            auditEvent.Route,
            auditEvent.Provider,
            auditEvent.Model,
            auditEvent.Outcome,
            auditEvent.LatencyMs,
            auditEvent.IdentityState,
            auditEvent.TenantId,
            auditEvent.PrincipalId,
            auditEvent.RequestContentState,
            auditEvent.ResponseContentState,
            auditEvent.CostEstimationStatus,
            auditEvent.EstimatedCost,
            auditEvent.EstimatedCostCurrency,
            auditEvent.CostEstimationSource,
            auditEvent.ReasonCode,
            auditEvent.InputTokens,
            auditEvent.OutputTokens);
        return Task.CompletedTask;
    }

    // Source-generated log message: no message content, only routing/cost/outcome metadata.
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "audit {EventType} route={Route} provider={Provider} model={Model} outcome={Outcome} latencyMs={LatencyMs} identityState={IdentityState} tenant={TenantId} principal={PrincipalId} requestContentState={RequestContentState} responseContentState={ResponseContentState} costStatus={CostEstimationStatus} estimatedCost={EstimatedCost} currency={EstimatedCostCurrency} costSource={CostEstimationSource} reason={ReasonCode} tokensIn={InputTokens} tokensOut={OutputTokens}")]
    private static partial void Audit(
        ILogger logger,
        string eventType,
        string route,
        string provider,
        string model,
        string outcome,
        long latencyMs,
        AuditIdentityState identityState,
        string tenantId,
        string principalId,
        AuditContentState requestContentState,
        AuditContentState responseContentState,
        AuditCostEstimationStatus costEstimationStatus,
        decimal? estimatedCost,
        string? estimatedCostCurrency,
        string costEstimationSource,
        string? reasonCode,
        int? inputTokens,
        int? outputTokens);
}
