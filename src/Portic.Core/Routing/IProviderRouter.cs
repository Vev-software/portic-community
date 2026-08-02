using Portic.Core.Contracts;
using Portic.Core.Providers;

namespace Portic.Core.Routing;

/// <summary>
/// Selects the <see cref="IChatProvider"/> adapter that should serve a given request. Routing policy
/// lives here so the SPI and adapters stay unaware of one another.
/// </summary>
public interface IProviderRouter
{
    IChatProvider Resolve(ChatRequest request);
}
