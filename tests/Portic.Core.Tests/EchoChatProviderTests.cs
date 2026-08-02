using Portic.Core.Contracts;
using Portic.Providers.Stub;
using Xunit;

namespace Portic.Core.Tests;

public sealed class EchoChatProviderTests
{
    private static ChatRequest Request(params (string Role, string Content)[] messages) => new()
    {
        Model = "stub-echo",
        Messages = messages.Select(m => new ChatMessage { Role = m.Role, Content = m.Content }).ToArray(),
    };

    [Fact]
    public async Task Echoes_last_user_message()
    {
        var provider = new EchoChatProvider();

        var completion = await provider.CompleteAsync(Request(("system", "be nice"), ("user", "hello world")));

        Assert.Equal("stub", completion.Provider);
        Assert.Equal("stub-echo", completion.Model);
        Assert.Equal("assistant", completion.Message.Role);
        Assert.Equal("echo: hello world", completion.Message.Content);
    }

    [Fact]
    public async Task Reports_nonzero_token_usage()
    {
        var provider = new EchoChatProvider();

        var completion = await provider.CompleteAsync(Request(("user", "one two three")));

        Assert.Equal(3, completion.Usage.InputTokens);
        Assert.True(completion.Usage.OutputTokens > 0);
        Assert.Equal(completion.Usage.InputTokens + completion.Usage.OutputTokens, completion.Usage.TotalTokens);
    }

    [Fact]
    public async Task Rejects_empty_conversation()
    {
        var provider = new EchoChatProvider();

        await Assert.ThrowsAsync<ArgumentException>(() => provider.CompleteAsync(Request()));
    }
}
