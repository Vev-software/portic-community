namespace Portic.Core.Contracts;

/// <summary>
/// A single normalized message in a conversation. Provider-neutral: adapters translate this to and
/// from their wire format. Roles are lower-case strings ("system", "user", "assistant") so the
/// contract does not bake in any one provider's enum.
/// </summary>
public sealed record ChatMessage
{
    public required string Role { get; init; }

    public required string Content { get; init; }
}
