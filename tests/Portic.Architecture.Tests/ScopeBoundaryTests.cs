using System.Text.RegularExpressions;
using Xunit;

namespace Portic.Architecture.Tests;

/// <summary>
/// FITNESS TEST -- enforces the scope boundary from portic-community#12. Portic Community core is an
/// AI gateway, not a full AI platform.
/// </summary>
public sealed class ScopeBoundaryTests
{
    private static readonly IReadOnlyList<string> OutOfCoreNameMarkers =
    [
        "AgentRuntime",
        "AgentsRuntime",
        "Rag",
        "RetrievalAugmentedGeneration",
        "DocumentProcessing",
        "McpRegistry",
    ];

    private static readonly Regex OutOfCorePublicType = new(
        @"\b(namespace|class|interface|record|struct|enum)\s+[A-Za-z0-9_.]*(AgentRuntime|AgentsRuntime|Rag|RetrievalAugmentedGeneration|DocumentProcessing|McpRegistry)[A-Za-z0-9_.]*\b",
        RegexOptions.Compiled);

    [Fact]
    public void Runtime_project_names_do_not_expand_into_out_of_core_platform_tracks()
    {
        var violations = RepoLayout.ProjectFiles()
            .Select(Path.GetFileNameWithoutExtension)
            .Where(projectName => projectName is not null && ContainsOutOfCoreMarker(projectName))
            .Select(projectName => $"{projectName} looks like an out-of-core platform track")
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Agent runtime, RAG, document processing, and MCP registry work must stay outside the Community core:\n  "
                + string.Join("\n  ", violations));
    }

    [Fact]
    public void Runtime_source_does_not_add_out_of_core_public_types()
    {
        var offenders = new List<string>();

        foreach (var file in SourceFiles())
        {
            var lines = File.ReadAllLines(file);
            var inBlockComment = false;
            for (var i = 0; i < lines.Length; i++)
            {
                var code = StripCommentsPreservingStrings(lines[i], ref inBlockComment);
                if (OutOfCorePublicType.IsMatch(code))
                {
                    offenders.Add($"{Path.GetRelativePath(RepoLayout.RepoRoot, file)}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Out-of-core platform tracks must not be added to the Community runtime source:\n  "
                + string.Join("\n  ", offenders));
    }

    private static bool ContainsOutOfCoreMarker(string name) =>
        OutOfCoreNameMarkers.Any(marker => name.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> SourceFiles() =>
        Directory.EnumerateFiles(RepoLayout.SrcDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
                !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    private static string StripCommentsPreservingStrings(string line, ref bool inBlockComment)
    {
        var result = new System.Text.StringBuilder(line.Length);
        var inString = false;
        var i = 0;

        while (i < line.Length)
        {
            var c = line[i];
            var next = i + 1 < line.Length ? line[i + 1] : '\0';

            if (inBlockComment)
            {
                if (c == '*' && next == '/') { inBlockComment = false; i += 2; }
                else { i++; }
            }
            else if (inString)
            {
                result.Append(c);
                if (c == '\\' && next != '\0') { result.Append(next); i += 2; }
                else { if (c == '"') { inString = false; } i++; }
            }
            else
            {
                if (c == '/' && next == '/') { break; }
                if (c == '/' && next == '*') { inBlockComment = true; i += 2; }
                else if (c == '"') { inString = true; result.Append(c); i++; }
                else { result.Append(c); i++; }
            }
        }

        return result.ToString();
    }
}
