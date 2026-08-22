using Vev.Fabric.Contracts;

namespace Portic.Core.Entitlements;

/// <summary>
/// Portic capability identifiers from the VEV taxonomy (fabric#4). Free gateway/control-plane core --
/// routing, provider governance, prompt libraries, basic AI audit -- does not pass through
/// entitlement; it is the hook (`13-Portic-Roadmap.md`). The <b>paid</b> capabilities below are
/// reserved so the entitlement seam exists before the features do (engineering#3, mirroring
/// atlas-community's <c>AtlasCapabilities</c>), and are denied in Community.
/// </summary>
public static class PorticCapabilities
{
    // --- Paid capabilities: reserved seams, entitlement-denied in Community ---

    /// <summary>Advanced request routing strategies beyond the default provider selection (paid, portic-enterprise).</summary>
    public static readonly CapabilityId AdvancedRouting = new("portic.routing.advanced");

    /// <summary>Governed policy management: model allowlists, PII redaction, per-team quotas (paid, portic-enterprise).</summary>
    public static readonly CapabilityId GovernancePolicy = new("portic.governance.policy");

    /// <summary>Centralized shadow-AI audit export and cost attribution across teams (paid, portic-enterprise).</summary>
    public static readonly CapabilityId GovernanceAuditExport = new("portic.governance.audit-export");

    /// <summary>
    /// The reserved paid capabilities, as one authoritative set. The free/paid line is entitlement-only:
    /// a Community-installed provider/extension may add value at the edges but may never declare or
    /// satisfy one of these -- that would be a back-door around the entitlement gate (engineering#3).
    /// </summary>
    public static readonly IReadOnlySet<CapabilityId> ReservedPaid = new HashSet<CapabilityId>
    {
        AdvancedRouting,
        GovernancePolicy,
        GovernanceAuditExport,
    };

    /// <summary>Whether <paramref name="capability"/> is a reserved paid capability no module may claim.</summary>
    public static bool IsReservedPaid(CapabilityId capability) => ReservedPaid.Contains(capability);
}
