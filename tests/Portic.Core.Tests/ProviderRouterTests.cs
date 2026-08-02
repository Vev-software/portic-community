using Microsoft.Extensions.Options;
using Portic.Core.Configuration;
using Portic.Core.Contracts;
using Portic.Core.Providers;
using Portic.Core.Routing;
using Xunit;

namespace Portic.Core.Tests;

public sealed class ProviderRouterTests
{
    private sealed class FakeProvider(string name) : IChatProvider
    {
        public string Name => name;

        public Task<ChatCompletion> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("routing test only");
    }

    private static ProviderRouter Router(string defaultProvider, params IChatProvider[] providers) =>
        new(providers, Options.Create(new GatewayOptions { DefaultProvider = defaultProvider }));

    private static ChatRequest Request(string? provider = null) => new()
    {
        Model = "m",
        Messages = [new ChatMessage { Role = "user", Content = "hi" }],
        Provider = provider,
    };

    [Fact]
    public void Falls_back_to_default_provider_when_none_requested()
    {
        var router = Router("stub", new FakeProvider("stub"), new FakeProvider("other"));

        Assert.Equal("stub", router.Resolve(Request()).Name);
    }

    [Fact]
    public void Honors_explicit_provider_case_insensitively()
    {
        var router = Router("stub", new FakeProvider("stub"), new FakeProvider("other"));

        Assert.Equal("other", router.Resolve(Request("OTHER")).Name);
    }

    [Fact]
    public void Throws_provider_not_found_for_unknown_provider()
    {
        var router = Router("stub", new FakeProvider("stub"));

        var ex = Assert.Throws<ProviderNotFoundException>(() => router.Resolve(Request("ghost")));
        Assert.Equal("ghost", ex.ProviderName);
    }
}
