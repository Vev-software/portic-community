using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Portic.Core.Costing;
using Portic.Core.Entitlements;
using Portic.Core.Governance;
using Portic.Core.Observability;
using Portic.Core.Providers;
using Portic.Core.Routing;
using Portic.Sdk.Contracts;
using Portic.Sdk.Providers;

namespace Portic.Core;

/// <summary>
/// Orchestrates one message request: policy → route → provider adapter → normalized completion,
/// wrapped in a telemetry span and a content-free audit event. All provider-specific behavior lives
/// behind <see cref="IChatProvider"/>; this class never references a provider SDK.
/// </summary>
public sealed partial class MessageGateway(
    GovernancePolicyGate policy,
    IProviderRouter router,
    IUsageCostEstimator costEstimator,
    IRequestContextAccessor context,
    IAuditSink auditSink,
    ILogger<MessageGateway> logger) : IMessageGateway
{
    private const string MessageRoute = "POST /v1/messages";

    public async Task<ChatCompletion> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity = PorticTelemetry.ActivitySource.StartActivity("ai.message");
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            policy.Enforce(request.Model);
        }
        catch (PolicyDeniedException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            await auditSink.RecordAsync(
                Failure(request.Model, provider: "n/a", ex.ReasonCode, startedAt, stopwatch.ElapsedMilliseconds),
                cancellationToken).ConfigureAwait(false);
            throw;
        }

        IChatProvider provider;
        try
        {
            provider = router.Resolve(request);
        }
        catch (ProviderNotFoundException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            await auditSink.RecordAsync(
                Failure(request.Model, ex.ProviderName, "provider_not_found", startedAt, stopwatch.ElapsedMilliseconds),
                cancellationToken).ConfigureAwait(false);
            throw;
        }

        // Tags carry routing/cost metadata only — never message content.
        activity?.SetTag("portic.provider", provider.Name);
        activity?.SetTag("portic.model", request.Model);

        try
        {
            var completion = await provider.CompleteAsync(request, cancellationToken).ConfigureAwait(false);
            var costEstimate = EstimateCost(provider.Name, completion);

            activity?.SetTag("portic.tokens.input", completion.Usage.InputTokens);
            activity?.SetTag("portic.tokens.output", completion.Usage.OutputTokens);

            await auditSink.RecordAsync(new AuditEvent
            {
                EventType = "ai.message.completed",
                Timestamp = startedAt,
                Route = MessageRoute,
                Provider = provider.Name,
                Model = completion.Model,
                Outcome = "success",
                LatencyMs = stopwatch.ElapsedMilliseconds,
                IdentityState = ResolveIdentityState(),
                TenantId = context.Tenant.TenantId,
                PrincipalId = context.Principal.PrincipalId,
                RequestContentState = AuditContentState.Withheld,
                ResponseContentState = AuditContentState.Withheld,
                InputTokens = completion.Usage.InputTokens,
                OutputTokens = completion.Usage.OutputTokens,
                CostEstimationStatus = costEstimate.Status,
                EstimatedCost = costEstimate.Amount,
                EstimatedCostCurrency = costEstimate.Currency,
                CostEstimationSource = costEstimate.Source,
            }, cancellationToken).ConfigureAwait(false);

            Completed(logger, provider.Name, completion.Model, completion.Usage.InputTokens, completion.Usage.OutputTokens);
            return completion;
        }
        catch (Exception ex) when (ex is not ProviderNotFoundException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            await auditSink.RecordAsync(
                Failure(request.Model, provider.Name, "provider_error", startedAt, stopwatch.ElapsedMilliseconds),
                cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private AuditEvent Failure(string model, string provider, string reasonCode, DateTimeOffset startedAt, long latencyMs) => new()
    {
        EventType = "ai.message.failed",
        Timestamp = startedAt,
        Route = MessageRoute,
        Provider = provider,
        Model = model,
        Outcome = "error",
        LatencyMs = latencyMs,
        IdentityState = ResolveIdentityState(),
        TenantId = context.Tenant.TenantId,
        PrincipalId = context.Principal.PrincipalId,
        RequestContentState = AuditContentState.Withheld,
        ResponseContentState = AuditContentState.Withheld,
        CostEstimationStatus = AuditCostEstimationStatus.NotComputed,
        EstimatedCost = null,
        EstimatedCostCurrency = null,
        CostEstimationSource = "not-computed",
        ReasonCode = reasonCode,
    };

    private AuditIdentityState ResolveIdentityState() =>
        context is SingleTenantRequestContextAccessor ? AuditIdentityState.Placeholder : AuditIdentityState.External;

    private UsageCostEstimate EstimateCost(string provider, ChatCompletion completion) =>
        costEstimator.Estimate(new UsageCostEstimationInput
        {
            Provider = provider,
            Model = completion.Model,
            InputTokens = completion.Usage.InputTokens,
            OutputTokens = completion.Usage.OutputTokens,
        });

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "message completed provider={Provider} model={Model} tokensIn={InputTokens} tokensOut={OutputTokens}")]
    private static partial void Completed(ILogger logger, string provider, string model, int inputTokens, int outputTokens);
}
