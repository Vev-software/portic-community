using Vev.Fabric.Contracts.Entitlements;

namespace Portic.Core.Entitlements;

/// <summary>
/// Community's entitlement evaluator: an empty grant set, always. Every reserved paid capability
/// (<see cref="PorticCapabilities.ReservedPaid"/>) is denied by construction -- there is no signed
/// snapshot, no remote lookup, and therefore no failure mode that could silently grant one
/// (fail-static, handbook 09 §4, E6). The free gateway/control-plane core never calls
/// <see cref="PaidCapabilityGate"/>, so it runs fully without passing through entitlement at all.
///
/// This is deliberately the smallest correct version of the seam -- mirroring the first cut of
/// atlas-community's own entitlement evaluator, before it grew signed-snapshot support for paid
/// deployments. A signed-snapshot-backed evaluator for portic-enterprise is separate, later work; the
/// point of this type is that the switch, when it comes, is a DI registration change, not a
/// call-site migration.
/// </summary>
public sealed class CommunityEntitlementService : IEntitlementService
{
    private const string CommunitySource = "entitlement:community-default";
    private readonly TimeProvider clock;

    public CommunityEntitlementService(TimeProvider? timeProvider = null)
    {
        clock = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public EntitlementDecision Evaluate(EntitlementRequest request) =>
        EntitlementDecision.Deny(request.Capability, ReasonCodes.EntitlementDenied, CommunitySource, clock.GetUtcNow());
}
