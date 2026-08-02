using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace Portic.Architecture.Tests;

/// <summary>
/// FITNESS TEST — enforces the primary AGENTS.md guardrail: "AI-native, never vendor-bound. No direct
/// AI-provider SDK calls anywhere outside a provider adapter."
///
/// Two independent layers, so the boundary is caught whether an SDK is merely declared or actually
/// linked:
///   1. <see cref="NonAdapter_projects_do_not_reference_provider_SDK_packages"/> — parses every
///      src/*.csproj (no build required) and fails if a non-adapter project declares a provider-SDK
///      PackageReference. Demonstrate a red run by adding e.g.
///      <c>&lt;PackageReference Include="OpenAI" ... /&gt;</c> to src/Portic.Gateway and running
///      <c>dotnet test tests/Portic.Architecture.Tests --no-build</c>.
///   2. <see cref="NonAdapter_assemblies_do_not_reference_provider_SDK_assemblies"/> — inspects the
///      compiled assemblies' referenced-assembly tables and fails if a non-adapter assembly links a
///      provider SDK (i.e. actually calls into it).
/// </summary>
public sealed class ProviderSdkBoundaryTests
{
    [Fact]
    public void NonAdapter_projects_do_not_reference_provider_SDK_packages()
    {
        var violations = new List<string>();

        foreach (var projectPath in RepoLayout.ProjectFiles())
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            if (ProviderSdkPolicy.IsProviderAdapter(projectName))
            {
                continue; // adapters are the one place provider SDKs are allowed
            }

            foreach (var packageId in PackageReferences(projectPath))
            {
                if (ProviderSdkPolicy.LooksLikeProviderSdk(packageId))
                {
                    violations.Add($"{projectName} references provider SDK package '{packageId}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Provider SDKs may only be referenced by Portic.Providers.* adapters. Violations:\n  "
                + string.Join("\n  ", violations));
    }

    [Fact]
    public void NonAdapter_assemblies_do_not_reference_provider_SDK_assemblies()
    {
        var violations = new List<string>();

        foreach (var assemblyPath in Directory.EnumerateFiles(AppContext.BaseDirectory, "Portic.*.dll"))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (ProviderSdkPolicy.IsProviderAdapter(assemblyName) || assemblyName.EndsWith(".Tests", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var referenced in Assembly.LoadFrom(assemblyPath).GetReferencedAssemblies())
            {
                if (ProviderSdkPolicy.LooksLikeProviderSdk(referenced.Name ?? string.Empty))
                {
                    violations.Add($"{assemblyName} links provider SDK assembly '{referenced.Name}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Non-adapter assemblies must not link an AI-provider SDK. Violations:\n  "
                + string.Join("\n  ", violations));
    }

    /// <summary>
    /// Guards the guard: proves the policy can actually distinguish a violation from clean code, so a
    /// broken scanner cannot silently pass the two tests above.
    /// </summary>
    [Fact]
    public void Policy_recognizes_violations_and_allowances()
    {
        Assert.True(ProviderSdkPolicy.LooksLikeProviderSdk("OpenAI"));
        Assert.True(ProviderSdkPolicy.LooksLikeProviderSdk("Azure.AI.OpenAI"));
        Assert.False(ProviderSdkPolicy.LooksLikeProviderSdk("Portic.Core"));

        Assert.True(ProviderSdkPolicy.IsProviderAdapter("Portic.Providers.Stub"));
        Assert.False(ProviderSdkPolicy.IsProviderAdapter("Portic.Gateway"));
    }

    private static IEnumerable<string> PackageReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        return document.Descendants()
            .Where(e => e.Name.LocalName == "PackageReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!);
    }
}
