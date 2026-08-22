using Vev.Fabric.Contracts;
using Vev.Fabric.Contracts.Entitlements;

namespace Portic.Core.Entitlements;

/// <summary>
/// The single, reusable seam through which every paid Portic capability is gated (mirrors
/// atlas-community's <c>PaidCapabilityGate</c>, engineering#3). Paid features live in the same
/// binary but ask the Fabric entitlement service whether the tenant may use them -- never
/// <c>if (plan == "…")</c> (AGENTS.md §1.4). Denial is clean and reason-coded; because the evaluator
/// is fail-static, an outage denies rather than silently grants (handbook 09 §4, E6).
/// </summary>
public sealed class PaidCapabilityGate(IRequestContextAccessor context, IEntitlementService entitlements)
{
    /// <summary>
    /// Evaluate the entitlement for the given capability, returning the <see cref="EntitlementDecision"/>
    /// so the caller can surface a reason-code-driven upgrade path instead of a broken experience.
    /// </summary>
    public EntitlementDecision Evaluate(CapabilityId capability, ResourceId? resource = null) =>
        entitlements.Evaluate(new EntitlementRequest(context.Tenant, capability, context.Principal, resource));

    /// <summary>Require the capability; throws <see cref="AccessDeniedException"/> when it is not granted.</summary>
    public void Require(CapabilityId capability, ResourceId? resource = null)
    {
        var decision = Evaluate(capability, resource);
        if (!decision.Allowed)
        {
            throw AccessDeniedException.FromEntitlement(decision, $"Capability '{capability}' is not enabled ({decision.ReasonCode}).");
        }
    }
}
