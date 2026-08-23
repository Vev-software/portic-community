using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Portic.Gateway.Endpoints;
using Portic.Sdk.Contracts;
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

    [Fact]
    public async Task Post_v1_messages_rejects_a_model_not_on_the_configured_allowlist()
    {
        var factoryWithAllowlist = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(
                [
                    new KeyValuePair<string, string?>("Portic:Policy:AllowedModels:0", "stub-approved"),
                ])));
        var client = factoryWithAllowlist.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("model_not_allowed", problem!.Title);
    }

    [Fact]
    public async Task Post_v1_messages_rejects_once_the_team_quota_is_exhausted()
    {
        var factoryWithQuota = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(
                [
                    new KeyValuePair<string, string?>("Portic:Policy:TeamDailyQuotas:portic-community-default", "1"),
                ])));
        var client = factoryWithQuota.CreateClient();
        var request = new ChatRequest
        {
            Model = "stub-echo",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        };

        var first = await client.PostAsJsonAsync("/v1/messages", request);
        var second = await client.PostAsJsonAsync("/v1/messages", request);

        first.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("quota_exceeded", problem!.Title);
    }

    [Fact]
    public async Task Get_v1_audit_recent_calls_returns_recent_gateway_traffic_without_raw_logs()
    {
        var client = _factory.CreateClient();

        var messageResponse = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        });

        messageResponse.EnsureSuccessStatusCode();

        var recentCalls = await client.GetFromJsonAsync<IReadOnlyList<RecentCallsEndpoints.RecentCallResponse>>("/v1/audit/recent-calls");

        var recentCall = Assert.Single(recentCalls!);
        Assert.Equal("POST /v1/messages", recentCall.Route);
        Assert.Equal("stub", recentCall.Provider);
        Assert.Equal("stub-echo", recentCall.Model);
        Assert.Equal("success", recentCall.Outcome);
        Assert.Equal("Withheld", recentCall.RequestContentState);
        Assert.Equal("Withheld", recentCall.ResponseContentState);
        Assert.Equal("UnknownPricing", recentCall.CostEstimationStatus);
        Assert.True(recentCall.InputTokens > 0);
        Assert.True(recentCall.OutputTokens > 0);
        Assert.True(recentCall.LatencyMs >= 0);
    }

    [Fact]
    public async Task Get_v1_audit_recent_calls_supports_provider_outcome_and_time_filters()
    {
        var client = _factory.CreateClient();

        var success = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        });
        var failure = await client.PostAsJsonAsync("/v1/messages", new ChatRequest
        {
            Model = "stub-echo",
            Provider = "ghost",
            Messages = [new ChatMessage { Role = "user", Content = "ping" }],
        });

        success.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.BadRequest, failure.StatusCode);

        var start = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        var end = DateTimeOffset.UtcNow.AddMinutes(1).ToString("O");
        var recentCalls = await client.GetFromJsonAsync<IReadOnlyList<RecentCallsEndpoints.RecentCallResponse>>(
            $"/v1/audit/recent-calls?provider=ghost&outcome=error&since={Uri.EscapeDataString(start)}&until={Uri.EscapeDataString(end)}");

        var recentCall = Assert.Single(recentCalls!);
        Assert.Equal("ghost", recentCall.Provider);
        Assert.Equal("error", recentCall.Outcome);
        Assert.Equal("provider_not_found", recentCall.ReasonCode);
        Assert.Equal("NotComputed", recentCall.CostEstimationStatus);
        Assert.Null(recentCall.InputTokens);
        Assert.Null(recentCall.OutputTokens);
    }
}
