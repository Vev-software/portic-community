namespace Portic.Core.Governance;

/// <summary>
/// Tracks and enforces the per-team daily request quota (<see cref="Portic.Core.Configuration.PolicyOptions.TeamDailyQuotas"/>).
/// </summary>
public interface ITeamQuotaEnforcer
{
    /// <summary>
    /// Record one request against <paramref name="team"/> and report whether it is within quota.
    /// Fail-safe: any internal error is treated as quota-exhausted (deny), never as unlimited (allow) --
    /// AGENTS.md "Fail-safe, not fail-open."
    /// </summary>
    bool TryConsume(string team);
}
