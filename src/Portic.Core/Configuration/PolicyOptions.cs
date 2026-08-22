namespace Portic.Core.Configuration;

/// <summary>
/// Governance policy configuration, bound from the "Portic:Policy" configuration section
/// (environment variables / appsettings). Core, free-tier gateway governance (portic-community#18,
/// `13-Portic-Roadmap.md`: "In core: ... governance, policy, ... spend control") -- not gated by
/// entitlement. The more advanced governance/policy tier (centralized cross-tenant management,
/// org-wide dashboards) is a separate, later, paid concern.
/// </summary>
public sealed class PolicyOptions
{
    public const string SectionName = "Portic:Policy";

    /// <summary>
    /// Models permitted to be requested. Empty (the default) means no allowlist is enforced -- every
    /// model is permitted, matching today's behavior. A non-empty list is a strict allowlist: a model
    /// not in it is denied, regardless of which provider would have served it.
    /// </summary>
    public IReadOnlyList<string> AllowedModels { get; set; } = [];

    /// <summary>
    /// Per-team daily request quota. The key is a team identifier (see
    /// <see cref="Portic.Core.Governance.GovernancePolicyGate"/> for how a team is resolved); a team
    /// with no entry here is unlimited. Empty (the default) means no team is quota-limited, matching
    /// today's behavior.
    /// </summary>
    public IReadOnlyDictionary<string, int> TeamDailyQuotas { get; set; } = new Dictionary<string, int>();
}
