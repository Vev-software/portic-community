using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Portic.Core.Configuration;

namespace Portic.Core.Governance;

/// <summary>
/// In-memory, per-process daily counter. Resets on restart and does not share state across
/// instances -- acceptable for the community, single-instance edition (AGENTS.md: "the request path
/// must never block on a control-plane DB"; a durable, shared quota store is hosted/enterprise scope).
/// </summary>
public sealed class InMemoryTeamQuotaEnforcer : ITeamQuotaEnforcer
{
    private readonly PolicyOptions options;
    private readonly TimeProvider clock;
    private readonly ConcurrentDictionary<string, (DateOnly Day, int Count)> usage = new(StringComparer.Ordinal);

    public InMemoryTeamQuotaEnforcer(IOptions<PolicyOptions> options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options.Value;
        clock = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public bool TryConsume(string team)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(team);

        try
        {
            if (!options.TeamDailyQuotas.TryGetValue(team, out var limit))
            {
                return true; // no quota configured for this team -- unlimited
            }

            var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
            var updated = usage.AddOrUpdate(
                team,
                _ => (today, 1),
                (_, existing) => existing.Day == today ? (today, existing.Count + 1) : (today, 1));

            return updated.Count <= limit;
        }
        catch
        {
            // Fail-safe: an unexpected error denies rather than silently allowing (AGENTS.md
            // "Fail-safe, not fail-open").
            return false;
        }
    }
}
