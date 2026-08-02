using Portic.Core.Contracts;
using Portic.Core.Providers;

namespace Portic.Providers.Stub;

/// <summary>
/// Local reference adapter. Echoes the last user message back as the assistant reply and estimates
/// token usage by word count. No API key, no network — this is what a clean clone runs by default,
/// and the golden example of how a real provider adapter (OpenAI, Anthropic, Ollama, …) plugs in
/// behind <see cref="IChatProvider"/> without the core knowing which one is present.
/// </summary>
public sealed class EchoChatProvider : IChatProvider
{
    public string Name => "stub";

    public Task<ChatCompletion> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("request must contain at least one message", nameof(request));
        }

        var prompt = LastUserContent(request.Messages);
        var replyText = $"echo: {prompt}";

        var completion = new ChatCompletion
        {
            Id = $"stub-{Guid.NewGuid():N}",
            Model = request.Model,
            Provider = Name,
            Message = new ChatMessage { Role = "assistant", Content = replyText },
            Usage = new TokenUsage
            {
                InputTokens = EstimateTokens(request.Messages),
                OutputTokens = EstimateTokens(replyText),
            },
        };

        return Task.FromResult(completion);
    }

    private static string LastUserContent(IReadOnlyList<ChatMessage> messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (string.Equals(messages[i].Role, "user", StringComparison.OrdinalIgnoreCase))
            {
                return messages[i].Content;
            }
        }

        return messages[^1].Content;
    }

    private static int EstimateTokens(IReadOnlyList<ChatMessage> messages)
    {
        var total = 0;
        foreach (var message in messages)
        {
            total += EstimateTokens(message.Content);
        }

        return total;
    }

    private static int EstimateTokens(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}
