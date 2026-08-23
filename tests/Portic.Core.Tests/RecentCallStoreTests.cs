using Portic.Core.Costing;
using Portic.Core.Observability;
using Portic.Core.RecentCalls;
using Xunit;

namespace Portic.Core.Tests;

public sealed class RecentCallStoreTests
{
    [Fact]
    public void Query_returns_most_recent_records_first()
    {
        var store = new BoundedRecentCallStore(capacity: 4);
        store.Append(Record("first", timestamp: new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero)));
        store.Append(Record("second", timestamp: new DateTimeOffset(2026, 8, 23, 10, 1, 0, TimeSpan.Zero)));
        store.Append(Record("third", timestamp: new DateTimeOffset(2026, 8, 23, 10, 2, 0, TimeSpan.Zero)));

        var records = store.Query(new RecentCallQuery());

        Assert.Collection(
            records,
            record => Assert.Equal("third", record.Provider),
            record => Assert.Equal("second", record.Provider),
            record => Assert.Equal("first", record.Provider));
    }

    [Fact]
    public void Query_evicts_oldest_records_when_capacity_is_exceeded()
    {
        var store = new BoundedRecentCallStore(capacity: 2);
        store.Append(Record("first", timestamp: new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero)));
        store.Append(Record("second", timestamp: new DateTimeOffset(2026, 8, 23, 10, 1, 0, TimeSpan.Zero)));
        store.Append(Record("third", timestamp: new DateTimeOffset(2026, 8, 23, 10, 2, 0, TimeSpan.Zero)));

        var records = store.Query(new RecentCallQuery());

        Assert.Collection(
            records,
            record => Assert.Equal("third", record.Provider),
            record => Assert.Equal("second", record.Provider));
    }

    [Fact]
    public void Query_filters_by_provider_model_outcome_and_time_window()
    {
        var store = new BoundedRecentCallStore(capacity: 5);
        store.Append(Record("openai", "gpt-4.1", "success", new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero)));
        store.Append(Record("anthropic", "claude-3", "success", new DateTimeOffset(2026, 8, 23, 10, 1, 0, TimeSpan.Zero)));
        store.Append(Record("openai", "gpt-4.1", "error", new DateTimeOffset(2026, 8, 23, 10, 2, 0, TimeSpan.Zero)));
        store.Append(Record("openai", "gpt-4o-mini", "success", new DateTimeOffset(2026, 8, 23, 10, 3, 0, TimeSpan.Zero)));

        var records = store.Query(new RecentCallQuery
        {
            Provider = "openai",
            Model = "gpt-4.1",
            Outcome = "success",
            Since = new DateTimeOffset(2026, 8, 23, 9, 59, 0, TimeSpan.Zero),
            Until = new DateTimeOffset(2026, 8, 23, 10, 0, 30, TimeSpan.Zero),
        });

        var record = Assert.Single(records);
        Assert.Equal("openai", record.Provider);
        Assert.Equal("gpt-4.1", record.Model);
        Assert.Equal("success", record.Outcome);
    }

    [Fact]
    public async Task Recent_call_audit_sink_projects_full_audit_metadata_into_the_read_model()
    {
        var store = new BoundedRecentCallStore(capacity: 4);
        var sink = new RecentCallAuditSink(store);
        var auditEvent = new AuditEvent
        {
            EventType = "ai.message.completed",
            Timestamp = new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero),
            Route = "POST /v1/messages",
            Provider = "openai",
            Model = "gpt-4.1",
            Outcome = "success",
            LatencyMs = 321,
            IdentityState = AuditIdentityState.External,
            TenantId = "tenant-1",
            PrincipalId = "user-1",
            RequestContentState = AuditContentState.Withheld,
            ResponseContentState = AuditContentState.Withheld,
            InputTokens = 12,
            OutputTokens = 34,
            CostEstimationStatus = AuditCostEstimationStatus.Estimated,
            EstimatedCost = 0.0042m,
            EstimatedCostCurrency = "USD",
            CostEstimationSource = "test-price-sheet",
            ReasonCode = null,
        };

        await sink.RecordAsync(auditEvent);

        var record = Assert.Single(store.Query(new RecentCallQuery()));
        Assert.Equal(auditEvent.EventType, record.EventType);
        Assert.Equal(auditEvent.Timestamp, record.Timestamp);
        Assert.Equal(auditEvent.Route, record.Route);
        Assert.Equal(auditEvent.Provider, record.Provider);
        Assert.Equal(auditEvent.Model, record.Model);
        Assert.Equal(auditEvent.Outcome, record.Outcome);
        Assert.Equal(auditEvent.LatencyMs, record.LatencyMs);
        Assert.Equal(auditEvent.IdentityState, record.IdentityState);
        Assert.Equal(auditEvent.TenantId, record.TenantId);
        Assert.Equal(auditEvent.PrincipalId, record.PrincipalId);
        Assert.Equal(auditEvent.RequestContentState, record.RequestContentState);
        Assert.Equal(auditEvent.ResponseContentState, record.ResponseContentState);
        Assert.Equal(auditEvent.InputTokens, record.InputTokens);
        Assert.Equal(auditEvent.OutputTokens, record.OutputTokens);
        Assert.Equal(auditEvent.CostEstimationStatus, record.CostEstimationStatus);
        Assert.Equal(auditEvent.EstimatedCost, record.EstimatedCost);
        Assert.Equal(auditEvent.EstimatedCostCurrency, record.EstimatedCostCurrency);
        Assert.Equal(auditEvent.CostEstimationSource, record.CostEstimationSource);
        Assert.Equal(auditEvent.ReasonCode, record.ReasonCode);
    }

    private static RecentCallRecord Record(
        string provider,
        string model = "stub-echo",
        string outcome = "success",
        DateTimeOffset? timestamp = null) =>
        new()
        {
            EventType = outcome == "success" ? "ai.message.completed" : "ai.message.failed",
            Timestamp = timestamp ?? new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero),
            Route = "POST /v1/messages",
            Provider = provider,
            Model = model,
            Outcome = outcome,
            LatencyMs = 10,
            IdentityState = AuditIdentityState.Placeholder,
            TenantId = "tenant",
            PrincipalId = "principal",
            RequestContentState = AuditContentState.Withheld,
            ResponseContentState = AuditContentState.Withheld,
            InputTokens = 1,
            OutputTokens = 2,
            CostEstimationStatus = AuditCostEstimationStatus.UnknownPricing,
            EstimatedCost = null,
            EstimatedCostCurrency = null,
            CostEstimationSource = "unknown",
            ReasonCode = outcome == "success" ? null : "provider_error",
        };
}
