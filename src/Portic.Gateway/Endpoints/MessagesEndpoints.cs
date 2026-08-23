using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Portic.Core;
using Portic.Core.Governance;
using Portic.Core.Providers;
using Portic.Sdk.Contracts;

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

    private static async Task<Results<Ok<ChatCompletion>, ProblemHttpResult>> HandleMessagesAsync(
        ChatRequest request,
        IMessageGateway gateway,
        CancellationToken cancellationToken)
    {
        if (request is null || request.Messages is null || request.Messages.Count == 0)
        {
            return Problem("messages_required", "At least one message is required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var completion = await gateway.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return TypedResults.Ok(completion);
        }
        catch (ProviderNotFoundException ex)
        {
            return Problem("provider_not_found", ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (PolicyDeniedException ex)
        {
            var status = ex.ReasonCode == "quota_exceeded" ? StatusCodes.Status429TooManyRequests : StatusCodes.Status403Forbidden;
            return Problem(ex.ReasonCode, ex.Message, status);
        }
    }

    private static ProblemHttpResult Problem(string reasonCode, string detail, int status) =>
        TypedResults.Problem(detail: detail, statusCode: status, title: reasonCode);
}
