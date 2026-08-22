using Vev.Fabric.Contracts.Entitlements;

namespace Portic.Core.Entitlements;

/// <summary>
/// Thrown when a Fabric entitlement decision denies an operation. Carries the reason code and source
/// so the API surfaces machine-readable denial context, never a bare 403 (mirrors
/// atlas-community's <c>AccessDeniedException</c>).
/// </summary>
public sealed class AccessDeniedException(AccessDeniedDetails decision, string message)
    : Exception(message)
{
    public static AccessDeniedException FromEntitlement(EntitlementDecision decision, string message) =>
        new(new AccessDeniedDetails(decision.ReasonCode, decision.Source), message);

    /// <summary>The denying decision, including reason code and source.</summary>
    public AccessDeniedDetails Decision { get; } = decision;
}

/// <summary>Minimal denial payload carried through the API boundary.</summary>
public sealed record AccessDeniedDetails(string ReasonCode, string Source);
