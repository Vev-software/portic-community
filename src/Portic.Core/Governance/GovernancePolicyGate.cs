using Microsoft.Extensions.Options;
using Portic.Core.Configuration;
using Portic.Core.Entitlements;

namespace Portic.Core.Governance;

/// <summary>
/// Core, free-tier governance gate: model allowlist + per-team quota. Deliberately separate from
/// <see cref="PaidCapabilityGate"/> -- this is core gateway governance
/// (`13-Portic-Roadmap.md`: "In core: ... governance, policy, ... spend control"), not an entitlement
/// decision. Enforcement is fail-safe: a denial is always a clean, reason-coded refusal, never a
/// silent pass-through.
/// </summary>
public sealed class GovernancePolicyGate(IOptions<PolicyOptions> options, ITeamQuotaEnforcer quotas, IRequestContextAccessor context)
{
    private readonly PolicyOptions options = options.Value;

    /// <summary>Team key a quota is tracked against: the principal's "team" claim, or the tenant if absent.</summary>
    public string ResolveTeam() =>
        context.Principal.Claims is { } claims && claims.TryGetValue("team", out var team) && !string.IsNullOrWhiteSpace(team)
            ? team
            : context.Tenant.TenantId;

    /// <summary>
    /// Enforce the model allowlist and the resolved team's quota. Throws <see cref="PolicyDeniedException"/>
    /// on denial; does nothing on success.
    /// </summary>
    public void Enforce(string model)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);

        if (options.AllowedModels.Count > 0 && !options.AllowedModels.Contains(model, StringComparer.OrdinalIgnoreCase))
        {
            throw new PolicyDeniedException("model_not_allowed", $"Model '{model}' is not on the configured allowlist.");
        }

        var team = ResolveTeam();
        if (!quotas.TryConsume(team))
        {
            throw new PolicyDeniedException("quota_exceeded", $"Team '{team}' has exceeded its daily request quota.");
        }
    }
}
