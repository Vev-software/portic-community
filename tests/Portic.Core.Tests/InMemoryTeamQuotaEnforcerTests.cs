using Microsoft.Extensions.Options;
using Portic.Core.Configuration;
using Portic.Core.Governance;
using Xunit;

namespace Portic.Core.Tests;

public sealed class InMemoryTeamQuotaEnforcerTests
{
    private static InMemoryTeamQuotaEnforcer Enforcer(
        IReadOnlyDictionary<string, int> quotas, TimeProvider? clock = null) =>
        new(Options.Create(new PolicyOptions { TeamDailyQuotas = quotas }), clock);

    [Fact]
    public void Allows_requests_up_to_the_configured_daily_limit()
    {
        var enforcer = Enforcer(new Dictionary<string, int> { ["team-a"] = 2 });

        Assert.True(enforcer.TryConsume("team-a"));
        Assert.True(enforcer.TryConsume("team-a"));
    }

    [Fact]
    public void Denies_once_the_daily_limit_is_exceeded()
    {
        var enforcer = Enforcer(new Dictionary<string, int> { ["team-a"] = 2 });

        Assert.True(enforcer.TryConsume("team-a"));
        Assert.True(enforcer.TryConsume("team-a"));
        Assert.False(enforcer.TryConsume("team-a"));
    }

    [Fact]
    public void Is_unlimited_for_a_team_with_no_configured_quota()
    {
        var enforcer = Enforcer(new Dictionary<string, int> { ["team-a"] = 1 });

        for (var i = 0; i < 10; i++)
        {
            Assert.True(enforcer.TryConsume("team-b"));
        }
    }

    [Fact]
    public void Tracks_teams_independently()
    {
        var enforcer = Enforcer(new Dictionary<string, int> { ["team-a"] = 1, ["team-b"] = 1 });

        Assert.True(enforcer.TryConsume("team-a"));
        Assert.True(enforcer.TryConsume("team-b"));
        Assert.False(enforcer.TryConsume("team-a"));
        Assert.False(enforcer.TryConsume("team-b"));
    }

    [Fact]
    public void Resets_the_counter_on_a_new_day()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var enforcer = Enforcer(new Dictionary<string, int> { ["team-a"] = 1 }, clock);

        Assert.True(enforcer.TryConsume("team-a"));
        Assert.False(enforcer.TryConsume("team-a"));

        clock.Advance(TimeSpan.FromDays(1));

        Assert.True(enforcer.TryConsume("team-a"));
    }

    private sealed class FakeTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public void Advance(TimeSpan by) => now += by;

        public override DateTimeOffset GetUtcNow() => now;
    }
}
