namespace Portic.Architecture.Tests;

/// <summary>
/// The single source of truth for the "AI-native, never vendor-bound" boundary: which package /
/// assembly names count as AI-provider SDKs, and which projects are permitted to touch them
/// (provider adapters only). Kept as pure, side-effect-free helpers so the tests are deterministic.
/// </summary>
internal static class ProviderSdkPolicy
{
    /// <summary>
    /// Case-insensitive substrings identifying AI-provider SDKs. A reference to any of these from a
    /// non-adapter package or assembly is a guardrail violation. Extend this list when a new provider
    /// SDK enters the ecosystem — it must stay ahead of what adapters actually use.
    /// </summary>
    public static readonly IReadOnlyList<string> ProviderSdkMarkers =
    [
        "OpenAI",
        "Azure.AI.OpenAI",
        "Anthropic",
        "Amazon.BedrockRuntime",
        "AWSSDK.BedrockRuntime",
        "Google.Cloud.AIPlatform",
        "Google.Apis.GenerativeLanguage",
        "Mistral",
        "Cohere",
        "OllamaSharp",
        "LLamaSharp",
        "Groq",
        "Replicate",
    ];

    /// <summary>
    /// A project/assembly is a provider ADAPTER (allowed to reference provider SDKs) iff its name is
    /// under the Portic.Providers.* namespace.
    /// </summary>
    public static bool IsProviderAdapter(string projectOrAssemblyName) =>
        projectOrAssemblyName.Contains("Portic.Providers.", StringComparison.OrdinalIgnoreCase);

    public static bool LooksLikeProviderSdk(string packageOrAssemblyName) =>
        ProviderSdkMarkers.Any(marker =>
            packageOrAssemblyName.Contains(marker, StringComparison.OrdinalIgnoreCase));
}
