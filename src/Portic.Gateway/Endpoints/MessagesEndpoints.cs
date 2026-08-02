using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Portic.Core;
using Portic.Core.Contracts;
using Portic.Core.Providers;

namespace Portic.Gateway.Endpoints;

/// <summary>
/// Maps the gateway's HTTP surface onto <see cref="IMessageGateway"/>. This is the only place that
/// knows about HTTP; the completion pipeline itself is host-agnostic.
/// </summary>
public static class MessagesEndpoints
{
    public static IEndpointRouteBuilder MapMessagesEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapPost("/v1/messages", HandleMessagesAsync)
            .WithName("CreateMessage");

        return app;
    }

    private static async Task<Results<Ok<ChatCompletion>, BadRequest<ProblemDetails>>> HandleMessagesAsync(
        ChatRequest request,
        IMessageGateway gateway,
        CancellationToken cancellationToken)
    {
        if (request is null || request.Messages is null || request.Messages.Count == 0)
        {
            return TypedResults.BadRequest(Problem("messages_required", "At least one message is required."));
        }

        try
        {
            var completion = await gateway.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return TypedResults.Ok(completion);
        }
        catch (ProviderNotFoundException ex)
        {
            return TypedResults.BadRequest(Problem("provider_not_found", ex.Message));
        }
    }

    private static ProblemDetails Problem(string reasonCode, string detail) => new()
    {
        Title = reasonCode,
        Detail = detail,
        Status = StatusCodes.Status400BadRequest,
    };
}
