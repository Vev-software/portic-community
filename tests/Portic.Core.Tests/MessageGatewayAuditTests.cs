using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portic.Core.Configuration;
using Portic.Core.Entitlements;
using Portic.Core.Governance;
using Portic.Core.Observability;
using Portic.Core.Providers;
using Portic.Core.Routing;
using Portic.Sdk.Contracts;
using Portic.Sdk.Providers;
using Vev.Fabric.Contracts;
using Xunit;

namespace Portic.Core.Tests;

public sealed class MessageGatewayAuditTests
{
    private sealed class RecordingAuditSink : IAuditSink
    {
        public List<AuditEvent> Events { get; } = [];

        public Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(auditEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class StaticContext(TenantContext tenant, PrincipalContext principal) : IRequestContextAccessor
    {
        public TenantContext Tenant { get; } = tenant;

        public PrincipalContext Principal { get; } = principal;
    }

    private sealed class FixedProvider(string name, Func<ChatRequest, ChatCompletion> complete) : IChatProvider
    {
        public string Name => name;

        public Task<ChatCompletion> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(complete(request));
    }

    private static MessageGateway CreateGateway(
        RecordingAuditSink sink,
        IRequestContextAccessor? context = null,
        IReadOnlyList<string>? allowedModels = null,
        params IChatProvider[] providers)
    {
        var policy = new GovernancePolicyGate(
            Options.Create(new PolicyOptions { AllowedModels = allowedModels ?? [] }),
            new AllowAllQuota(),
            context ?? new SingleTenantRequestContextAccessor());
        var router = new ProviderRouter(
            providers,
            Options.Create(new GatewayOptions { DefaultProvider = "stub" }));

        return new MessageGateway(
            policy,
            router,
            context ?? new SingleTenantRequestContextAccessor(),
            sink,
            NullLogger<MessageGateway>.Instance);
    }

    private static ChatRequest Request(string model = "stub-echo", string? provider = null) => new()
    {
        Model = model,
        Provider = provider,
        Messages = [new ChatMessage { Role = "user", Content = "ping" }],
    };

    [Fact]
    public async Task Completed_calls_emit_extended_audit_metadata_without_content()
    {
        var sink = new RecordingAuditSink();
        var context = new StaticContext(new TenantContext("tenant-1"), new PrincipalContext("user-1", DisplayName: null, Roles: []));
        var provider = new FixedProvider("stub", request => new ChatCompletion
        {
            Id = "completion-1",
            Model = request.Model,
            Provider = "stub",
            Message = new ChatMessage { Role = "assistant", Content = "echo: ping" },
            Usage = new TokenUsage { InputTokens = 2, OutputTokens = 3 },
        });
        var gateway = CreateGateway(sink, context, providers: [provider]);

        var completion = await gateway.SendAsync(Request());

        Assert.Equal("stub", completion.Provider);
        var audit = Assert.Single(sink.Events);
        Assert.Equal("ai.message.completed", audit.EventType);
        Assert.Equal("POST /v1/messages", audit.Route);
        Assert.Equal("success", audit.Outcome);
        Assert.Equal("stub", audit.Provider);
        Assert.Equal("stub-echo", audit.Model);
        Assert.Equal(2, audit.InputTokens);
        Assert.Equal(3, audit.OutputTokens);
        Assert.Equal("tenant-1", audit.TenantId);
        Assert.Equal("user-1", audit.PrincipalId);
        Assert.Equal(AuditIdentityState.External, audit.IdentityState);
        Assert.Equal(AuditContentState.Withheld, audit.RequestContentState);
        Assert.Equal(AuditContentState.Withheld, audit.ResponseContentState);
        Assert.True(audit.LatencyMs >= 0);
        Assert.Null(audit.ReasonCode);
    }

    [Fact]
    public async Task Unknown_provider_failures_emit_the_same_core_metadata_shape()
    {
        var sink = new RecordingAuditSink();
        var gateway = CreateGateway(sink);

        var ex = await Assert.ThrowsAsync<ProviderNotFoundException>(() => gateway.SendAsync(Request(provider: "ghost")));

        Assert.Equal("ghost", ex.ProviderName);
        var audit = Assert.Single(sink.Events);
        Assert.Equal("ai.message.failed", audit.EventType);
        Assert.Equal("POST /v1/messages", audit.Route);
        Assert.Equal("error", audit.Outcome);
        Assert.Equal("ghost", audit.Provider);
        Assert.Equal("stub-echo", audit.Model);
        Assert.Equal("provider_not_found", audit.ReasonCode);
        Assert.Equal(SingleTenantRequestContextAccessor.DefaultTenantId, audit.TenantId);
        Assert.Equal(SingleTenantRequestContextAccessor.DefaultPrincipalId, audit.PrincipalId);
        Assert.Equal(AuditIdentityState.Placeholder, audit.IdentityState);
        Assert.Equal(AuditContentState.Withheld, audit.RequestContentState);
        Assert.Equal(AuditContentState.Withheld, audit.ResponseContentState);
        Assert.True(audit.LatencyMs >= 0);
        Assert.Null(audit.InputTokens);
        Assert.Null(audit.OutputTokens);
    }

    [Fact]
    public async Task Policy_denials_still_throw_after_recording_fail_safe_metadata()
    {
        var sink = new RecordingAuditSink();
        var provider = new FixedProvider("stub", request => new ChatCompletion
        {
            Id = "completion-1",
            Model = request.Model,
            Provider = "stub",
            Message = new ChatMessage { Role = "assistant", Content = "echo: ping" },
            Usage = new TokenUsage { InputTokens = 1, OutputTokens = 1 },
        });
        var gateway = CreateGateway(sink, allowedModels: ["allowed-model"], providers: [provider]);

        var ex = await Assert.ThrowsAsync<PolicyDeniedException>(() => gateway.SendAsync(Request(model: "denied-model")));

        Assert.Equal("model_not_allowed", ex.ReasonCode);
        var audit = Assert.Single(sink.Events);
        Assert.Equal("ai.message.failed", audit.EventType);
        Assert.Equal("n/a", audit.Provider);
        Assert.Equal("model_not_allowed", audit.ReasonCode);
        Assert.Equal("POST /v1/messages", audit.Route);
        Assert.Equal(AuditIdentityState.Placeholder, audit.IdentityState);
        Assert.True(audit.LatencyMs >= 0);
    }

    private sealed class AllowAllQuota : ITeamQuotaEnforcer
    {
        public bool TryConsume(string team) => true;
    }
}
