using Microsoft.Extensions.Options;
using Portic.Core.Configuration;
using Portic.Core.Entitlements;
using Portic.Core.Governance;
using Vev.Fabric.Contracts;
using Xunit;

namespace Portic.Core.Tests;

public sealed class GovernancePolicyGateTests
{
    private sealed class FixedQuota(bool allowed) : ITeamQuotaEnforcer
    {
        public bool TryConsume(string team) => allowed;
    }

    private sealed class StaticContext(TenantContext tenant, PrincipalContext principal) : IRequestContextAccessor
    {
        public TenantContext Tenant { get; } = tenant;

        public PrincipalContext Principal { get; } = principal;
    }

    private static GovernancePolicyGate Gate(
        IReadOnlyList<string>? allowedModels = null,
        bool quotaAllowed = true,
        IRequestContextAccessor? context = null) =>
        new(
            Options.Create(new PolicyOptions { AllowedModels = allowedModels ?? [] }),
            new FixedQuota(quotaAllowed),
            context ?? new SingleTenantRequestContextAccessor());

    [Fact]
    public void Allows_any_model_when_no_allowlist_is_configured()
    {
        var gate = Gate();

        var exception = Record.Exception(() => gate.Enforce("gpt-anything"));

        Assert.Null(exception);
    }

    [Fact]
    public void Denies_a_model_not_on_the_configured_allowlist()
    {
        var gate = Gate(allowedModels: ["gpt-approved"]);

        var ex = Assert.Throws<PolicyDeniedException>(() => gate.Enforce("gpt-unapproved"));

        Assert.Equal("model_not_allowed", ex.ReasonCode);
    }

    [Fact]
    public void Allows_a_model_on_the_configured_allowlist()
    {
        var gate = Gate(allowedModels: ["gpt-approved"]);

        var exception = Record.Exception(() => gate.Enforce("gpt-approved"));

        Assert.Null(exception);
    }

    [Fact]
    public void Denies_when_the_team_quota_is_exhausted()
    {
        var gate = Gate(quotaAllowed: false);

        var ex = Assert.Throws<PolicyDeniedException>(() => gate.Enforce("any-model"));

        Assert.Equal("quota_exceeded", ex.ReasonCode);
    }

    [Fact]
    public void Resolves_team_from_the_principal_team_claim_when_present()
    {
        var context = new StaticContext(
            new TenantContext("tenant-1"),
            new PrincipalContext("user-1", DisplayName: null, Roles: [], Claims: new Dictionary<string, string> { ["team"] = "platform" }));
        var gate = Gate(context: context);

        Assert.Equal("platform", gate.ResolveTeam());
    }

    [Fact]
    public void Falls_back_to_the_tenant_when_no_team_claim_is_present()
    {
        var context = new StaticContext(new TenantContext("tenant-1"), new PrincipalContext("user-1", DisplayName: null, Roles: []));
        var gate = Gate(context: context);

        Assert.Equal("tenant-1", gate.ResolveTeam());
    }
}
