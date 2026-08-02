using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Portic.Core.Contracts;
using Portic.Core.Observability;
using Portic.Core.Providers;
using Portic.Core.Routing;

namespace Portic.Core;

/// <summary>
/// Orchestrates one message request: route → provider adapter → normalized completion, wrapped in a
/// telemetry span and a content-free audit event. All provider-specific behavior lives behind
/// <see cref="IChatProvider"/>; this class never references a provider SDK.
/// </summary>
public sealed partial class MessageGateway(
    IProviderRouter router,
    IAuditSink auditSink,
    ILogger<MessageGateway> logger) : IMessageGateway
{
    public async Task<ChatCompletion> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity = PorticTelemetry.ActivitySource.StartActivity("ai.message");

        IChatProvider provider;
        try
        {
            provider = router.Resolve(request);
        }
        catch (ProviderNotFoundException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            await auditSink.RecordAsync(
                Failure(request.Model, ex.ProviderName, "provider_not_found"),
                cancellationToken).ConfigureAwait(false);
            throw;
        }

        // Tags carry routing/cost metadata only — never message content.
        activity?.SetTag("portic.provider", provider.Name);
        activity?.SetTag("portic.model", request.Model);

        try
        {
            var completion = await provider.CompleteAsync(request, cancellationToken).ConfigureAwait(false);

            activity?.SetTag("portic.tokens.input", completion.Usage.InputTokens);
            activity?.SetTag("portic.tokens.output", completion.Usage.OutputTokens);

            await auditSink.RecordAsync(new AuditEvent
            {
                EventType = "ai.message.completed",
                Timestamp = DateTimeOffset.UtcNow,
                Provider = provider.Name,
                Model = completion.Model,
                Outcome = "success",
                InputTokens = completion.Usage.InputTokens,
                OutputTokens = completion.Usage.OutputTokens,
            }, cancellationToken).ConfigureAwait(false);

            Completed(logger, provider.Name, completion.Model, completion.Usage.InputTokens, completion.Usage.OutputTokens);
            return completion;
        }
        catch (Exception ex) when (ex is not ProviderNotFoundException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            await auditSink.RecordAsync(
                Failure(request.Model, provider.Name, "provider_error"),
                cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static AuditEvent Failure(string model, string provider, string reasonCode) => new()
    {
        EventType = "ai.message.failed",
        Timestamp = DateTimeOffset.UtcNow,
        Provider = provider,
        Model = model,
        Outcome = "error",
        ReasonCode = reasonCode,
    };

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "message completed provider={Provider} model={Model} tokensIn={InputTokens} tokensOut={OutputTokens}")]
    private static partial void Completed(ILogger logger, string provider, string model, int inputTokens, int outputTokens);
}
