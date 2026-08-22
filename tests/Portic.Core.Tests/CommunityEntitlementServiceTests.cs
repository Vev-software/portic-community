using Portic.Core.Entitlements;
using Vev.Fabric.Contracts;
using Vev.Fabric.Contracts.Entitlements;
using Xunit;

namespace Portic.Core.Tests;

public sealed class CommunityEntitlementServiceTests
{
    private static readonly TenantContext Tenant = new("tenant-1");
    private static readonly PrincipalContext Principal = new("user-1", DisplayName: null, Roles: []);

    [Fact]
    public void Denies_every_capability_by_default()
    {
        var service = new CommunityEntitlementService();

        var decision = service.Evaluate(new EntitlementRequest(Tenant, new CapabilityId("anything.at.all"), Principal));

        Assert.False(decision.Allowed);
        Assert.Equal(ReasonCodes.EntitlementDenied, decision.ReasonCode);
    }

    [Fact]
    public void Denies_every_reserved_paid_capability()
    {
        var service = new CommunityEntitlementService();

        foreach (var capability in PorticCapabilities.ReservedPaid)
        {
            var decision = service.Evaluate(new EntitlementRequest(Tenant, capability, Principal));
            Assert.False(decision.Allowed, $"{capability} must be denied in Community");
        }
    }

    [Fact]
    public void Is_fail_static_not_fail_open_there_is_no_configuration_that_grants_anything()
    {
        // There is no snapshot, no remote source, no config to point at -- unlike a real fail-static
        // evaluator, Community has no failure mode at all: it always denies. This test exists so a
        // future change that adds a "grant if X" branch here has to consciously break it.
        var service = new CommunityEntitlementService();

        var decision = service.Evaluate(new EntitlementRequest(Tenant, PorticCapabilities.AdvancedRouting, Principal));

        Assert.False(decision.Allowed);
    }
}
