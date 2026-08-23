using Microsoft.Extensions.Options;
using Portic.Core.Configuration;
using Portic.Core.Providers;
using Portic.Sdk.Contracts;
using Portic.Sdk.Providers;

namespace Portic.Core.Routing;

/// <summary>
/// Default router: resolves an adapter by the request's explicit <see cref="ChatRequest.Provider"/>,
/// falling back to the configured default provider. Adapters are discovered purely through DI — the
/// router has no compile-time knowledge of any concrete provider.
/// </summary>
public sealed class ProviderRouter : IProviderRouter
{
    private readonly Dictionary<string, IChatProvider> _providers;
    private readonly string _defaultProvider;

    public ProviderRouter(IEnumerable<IChatProvider> providers, IOptions<GatewayOptions> options)
    {
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(options);

        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _defaultProvider = options.Value.DefaultProvider;
    }

    public IChatProvider Resolve(ChatRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = string.IsNullOrWhiteSpace(request.Provider) ? _defaultProvider : request.Provider;

        return _providers.TryGetValue(name, out var provider)
            ? provider
            : throw new ProviderNotFoundException(name);
    }
}
