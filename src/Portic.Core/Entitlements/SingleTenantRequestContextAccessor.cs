using Vev.Fabric.Contracts;

namespace Portic.Core.Entitlements;

/// <summary>
/// Community placeholder: every request resolves to one fixed tenant and an anonymous principal.
/// This is a stand-in for real Fabric identity (no authentication happens here), matching the
/// self-hosted, single-tenant posture the free edition ships with today -- the same shape as Atlas
/// Community's own header-shim-then-real-identity evolution. Replace the DI registration with a real
/// Fabric identity accessor when one is wired in; <see cref="Portic.Core.Entitlements.PaidCapabilityGate"/>
/// and every other consumer of <see cref="IRequestContextAccessor"/> do not change.
/// </summary>
public sealed class SingleTenantRequestContextAccessor : IRequestContextAccessor
{
    public const string DefaultTenantId = "portic-community-default";
    public const string DefaultPrincipalId = "anonymous";

    public TenantContext Tenant { get; } = new(DefaultTenantId);

    public PrincipalContext Principal { get; } = new(DefaultPrincipalId, DisplayName: null, Roles: []);
}
