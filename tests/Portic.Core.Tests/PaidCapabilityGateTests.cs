using Portic.Core.Entitlements;
using Vev.Fabric.Contracts;
using Vev.Fabric.Contracts.Entitlements;
using Xunit;

namespace Portic.Core.Tests;

public sealed class PaidCapabilityGateTests
{
    private sealed class FixedDecisionEntitlementService(bool allowed) : IEntitlementService
    {
        public EntitlementDecision Evaluate(EntitlementRequest request) => allowed
            ? EntitlementDecision.Allow(request.Capability, "test", DateTimeOffset.UtcNow)
            : EntitlementDecision.Deny(request.Capability, ReasonCodes.EntitlementDenied, "test", DateTimeOffset.UtcNow);
    }

    private static PaidCapabilityGate GateReturning(bool allowed) =>
        new(new SingleTenantRequestContextAccessor(), new FixedDecisionEntitlementService(allowed));

    [Fact]
    public void Require_throws_AccessDeniedException_when_capability_is_not_granted()
    {
        var gate = GateReturning(allowed: false);

        var ex = Assert.Throws<AccessDeniedException>(() => gate.Require(PorticCapabilities.AdvancedRouting));

        Assert.Equal(ReasonCodes.EntitlementDenied, ex.Decision.ReasonCode);
    }

    [Fact]
    public void Require_does_not_throw_when_capability_is_granted()
    {
        var gate = GateReturning(allowed: true);

        var exception = Record.Exception(() => gate.Require(PorticCapabilities.AdvancedRouting));

        Assert.Null(exception);
    }

    [Fact]
    public void Evaluate_passes_the_ambient_tenant_and_principal_through()
    {
        var context = new SingleTenantRequestContextAccessor();
        EntitlementRequest? captured = null;
        var service = new CapturingEntitlementService(request => captured = request);
        var gate = new PaidCapabilityGate(context, service);

        gate.Evaluate(PorticCapabilities.GovernancePolicy);

        Assert.NotNull(captured);
        Assert.Equal(context.Tenant, captured!.Value.Tenant);
        Assert.Equal(context.Principal, captured.Value.Principal);
        Assert.Equal(PorticCapabilities.GovernancePolicy, captured.Value.Capability);
    }

    private sealed class CapturingEntitlementService(Action<EntitlementRequest> capture) : IEntitlementService
    {
        public EntitlementDecision Evaluate(EntitlementRequest request)
        {
            capture(request);
            return EntitlementDecision.Deny(request.Capability, ReasonCodes.EntitlementDenied, "test", DateTimeOffset.UtcNow);
        }
    }
}
