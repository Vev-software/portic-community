using Portic.Sdk.Contracts;

namespace Portic.Core;

/// <summary>
/// Application-facing entry point for a chat completion. This is the reusable core of the gateway:
/// the HTTP endpoint, a future CLI, or the SDK all drive the same service (AGENTS.md: "API/SDK first;
/// the UI is never the only path").
/// </summary>
public interface IMessageGateway
{
    Task<ChatCompletion> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
