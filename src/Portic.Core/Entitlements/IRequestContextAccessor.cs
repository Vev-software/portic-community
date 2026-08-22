using Vev.Fabric.Contracts;

namespace Portic.Core.Entitlements;

/// <summary>
/// Ambient accessor for the tenant + principal bound to the current request. Identity is a Fabric
/// concern (AGENTS.md: "Never re-implement a Fabric concern... Identity, tenancy... → Fabric"); this
/// is the port Portic depends on, mirroring Atlas's own <c>IRequestContextAccessor</c>.
///
/// The community edition ships only <see cref="SingleTenantRequestContextAccessor"/> -- a single
/// fixed tenant, no real authentication -- as an explicit placeholder, the same way <c>IAuditSink</c>
/// ships only <c>LoggingAuditSink</c> pending a real Fabric identity contract (ADR-0002). Swapping in
/// real Fabric OIDC identity later is a registration change in <c>AddPorticCore</c>, not a call-site
/// change.
/// </summary>
public interface IRequestContextAccessor
{
    /// <summary>The tenant bound to the current request.</summary>
    TenantContext Tenant { get; }

    /// <summary>The principal bound to the current request.</summary>
    PrincipalContext Principal { get; }
}
