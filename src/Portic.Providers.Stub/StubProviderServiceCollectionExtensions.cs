using Microsoft.Extensions.DependencyInjection;
using Portic.Sdk.Providers;

namespace Portic.Providers.Stub;

/// <summary>
/// Registers the local stub adapter. Swapping providers is a one-line change at composition root:
/// replace <c>AddStubProvider()</c> with a real adapter's registration; nothing in the core changes.
/// </summary>
public static class StubProviderServiceCollectionExtensions
{
    public static IServiceCollection AddStubProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IChatProvider, EchoChatProvider>();
        return services;
    }
}
