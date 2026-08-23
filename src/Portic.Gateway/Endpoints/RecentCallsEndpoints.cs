using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Portic.Core.RecentCalls;

namespace Portic.Gateway.Endpoints;

/// <summary>
/// Exposes the Community recent-call read model over HTTP. This is a thin, in-process visibility
/// surface over recent traffic through Portic itself, not a discovery surface outside the gateway.
/// </summary>
public static class RecentCallsEndpoints
{
    public static IEndpointRouteBuilder MapRecentCallsEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/v1/audit/recent-calls", HandleRecentCalls)
            .WithName("ListRecentCalls");

        return app;
    }

    private static Ok<RecentCallResponse[]> HandleRecentCalls(
        [AsParameters] RecentCallRequest request,
        IRecentCallQueryService queryService)
    {
        var records = queryService.QueryRecentCalls(new RecentCallQuery
        {
            Provider = request.Provider,
            Model = request.Model,
            Outcome = request.Outcome,
            Since = request.Since,
            Until = request.Until,
        });

        return TypedResults.Ok(records.Select(record => new RecentCallResponse
        {
            Timestamp = record.Timestamp,
            EventType = record.EventType,
            Route = record.Route,
            Provider = record.Provider,
            Model = record.Model,
            Outcome = record.Outcome,
            LatencyMs = record.LatencyMs,
            TenantId = record.TenantId,
            PrincipalId = record.PrincipalId,
            IdentityState = record.IdentityState.ToString(),
            RequestContentState = record.RequestContentState.ToString(),
            ResponseContentState = record.ResponseContentState.ToString(),
            InputTokens = record.InputTokens,
            OutputTokens = record.OutputTokens,
            CostEstimationStatus = record.CostEstimationStatus.ToString(),
            EstimatedCost = record.EstimatedCost,
            EstimatedCostCurrency = record.EstimatedCostCurrency,
            CostEstimationSource = record.CostEstimationSource,
            ReasonCode = record.ReasonCode,
        }).ToArray());
    }

    public sealed record RecentCallRequest
    {
        [FromQuery(Name = "provider")]
        public string? Provider { get; init; }

        [FromQuery(Name = "model")]
        public string? Model { get; init; }

        [FromQuery(Name = "outcome")]
        public string? Outcome { get; init; }

        [FromQuery(Name = "since")]
        public DateTimeOffset? Since { get; init; }

        [FromQuery(Name = "until")]
        public DateTimeOffset? Until { get; init; }
    }

    public sealed record RecentCallResponse
    {
        public required DateTimeOffset Timestamp { get; init; }

        public required string EventType { get; init; }

        public required string Route { get; init; }

        public required string Provider { get; init; }

        public required string Model { get; init; }

        public required string Outcome { get; init; }

        public required long LatencyMs { get; init; }

        public required string TenantId { get; init; }

        public required string PrincipalId { get; init; }

        public required string IdentityState { get; init; }

        public required string RequestContentState { get; init; }

        public required string ResponseContentState { get; init; }

        public int? InputTokens { get; init; }

        public int? OutputTokens { get; init; }

        public required string CostEstimationStatus { get; init; }

        public decimal? EstimatedCost { get; init; }

        public string? EstimatedCostCurrency { get; init; }

        public required string CostEstimationSource { get; init; }

        public string? ReasonCode { get; init; }
    }
}
