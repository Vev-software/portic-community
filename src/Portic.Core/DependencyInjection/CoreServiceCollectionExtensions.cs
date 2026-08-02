using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portic.Core.Configuration;
using Portic.Core.Observability;
using Portic.Core.Routing;

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

        services.AddSingleton<IProviderRouter, ProviderRouter>();
        services.AddSingleton<IAuditSink, LoggingAuditSink>();
        services.AddSingleton<IMessageGateway, MessageGateway>();

        return services;
    }
}
