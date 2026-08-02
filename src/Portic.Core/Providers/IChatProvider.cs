using Portic.Core.Contracts;

namespace Portic.Core.Providers;

/// <summary>
/// The provider SPI (Service Provider Interface) — the permanent AI contract every model call goes
/// through. This is the ports-and-adapters "port": Portic.Core depends only on this interface, and
/// each concrete provider (OpenAI, Anthropic, Ollama, a local stub, …) is a disposable adapter that
/// implements it inside a <c>Portic.Providers.*</c> project.
///
/// GUARDRAIL (AGENTS.md / fitness-tested): no AI-provider SDK may be referenced or called anywhere
/// except inside a provider adapter that implements this interface.
///
/// TODO(ADR-0001): this SPI is a natural fit for the Apache-2.0 <c>portic-sdk</c> repo so external
/// integrators can implement providers without taking an AGPL runtime dependency. It is stubbed here
/// pending that decision — see docs/adr/0001-provider-spi-location.md.
/// </summary>
public interface IChatProvider
{
    /// <summary>Stable, lower-case provider name used for routing and audit, e.g. "stub".</summary>
    string Name { get; }

    /// <summary>Serve a normalized request and return a normalized completion.</summary>
    Task<ChatCompletion> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
