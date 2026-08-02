using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Portic.Core.Contracts;
using Xunit;

namespace Portic.Gateway.IntegrationTests;

/// <summary>
/// Boots the real gateway host in-process (WebApplicationFactory) and drives POST /v1/messages over
/// HTTP against the default stub provider — no external API key, no network.
/// </summary>
public sealed class MessagesEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task Post_v1_messages_returns_stub_completion()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        });

        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<ChatCompletion>();
        Assert.NotNull(completion);
        Assert.Equal("stub", completion!.Provider);
        Assert.Equal("echo: ping", completion.Message.Content);
        Assert.True(completion.Usage.TotalTokens > 0);
    }

    [Fact]
    public async Task Post_v1_messages_rejects_empty_messages()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Messages = [],
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_v1_messages_rejects_unknown_provider()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Provider = "ghost",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
