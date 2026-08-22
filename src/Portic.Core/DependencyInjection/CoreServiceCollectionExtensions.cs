using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portic.Core.Configuration;
using Portic.Core.Entitlements;
using Portic.Core.Governance;
using Portic.Core.Observability;
using Portic.Core.Routing;
using Vev.Fabric.Contracts.Entitlements;

namespace Portic.Core.DependencyInjection;

/// <summary>
/// Registers the provider-neutral gateway core. Providers are added separately via their own adapter
/// packages (e.g. <c>AddStubProvider()</c>) so the core never depends on a concrete provider.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddPorticCore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<GatewayOptions>()
            .Bind(configuration.GetSection(GatewayOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<PolicyOptions>()
            .Bind(configuration.GetSection(PolicyOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IProviderRouter, ProviderRouter>();
        services.AddSingleton<IAuditSink, LoggingAuditSink>();
        services.AddSingleton<IMessageGateway, MessageGateway>();

        // Entitlement seam (engineering#3): Community ships the fail-static, empty-grant-set
        // evaluator and a single-tenant identity placeholder. Swapping either for a real Fabric
        // identity/entitlement source is a registration change here, not a call-site change.
        services.AddSingleton<IRequestContextAccessor, SingleTenantRequestContextAccessor>();
        services.AddSingleton<IEntitlementService, CommunityEntitlementService>();
        services.AddSingleton<PaidCapabilityGate>();

        // Core governance policy (portic-community#18): model allowlist + per-team quota. Free-tier,
        // not entitlement-gated -- see GovernancePolicyGate's own remarks on why this is distinct
        // from PaidCapabilityGate.
        services.AddSingleton<IContentRedactor, RegexPiiRedactor>();
        services.AddSingleton<ITeamQuotaEnforcer, InMemoryTeamQuotaEnforcer>();
        services.AddSingleton<GovernancePolicyGate>();

        return services;
    }
}
