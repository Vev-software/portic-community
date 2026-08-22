using System.Text.RegularExpressions;
using Xunit;

namespace Portic.Architecture.Tests;

/// <summary>
/// Source-level fitness check for a rule that is about *how code is written*, not just assembly
/// references (AGENTS.md §1.4, engineering#3): the free/paid line must be an entitlement decision
/// from <see cref="Portic.Core.Entitlements.PaidCapabilityGate"/>, never a hand-rolled plan check.
/// Mirrors atlas-community's <c>ForbiddenPatternTests</c>.
/// </summary>
public sealed class ForbiddenPatternTests
{
    // `if (plan == "enterprise")` and friends.
    private static readonly Regex PlanCheck = new(
        """\bplan\b\s*==\s*["']""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void No_plan_equality_checks_anywhere_in_the_source()
    {
        var offenders = ScanFor(PlanCheck);
        Assert.True(offenders.Count == 0,
            "The free/paid line must be an entitlement decision (PaidCapabilityGate), never `if (plan == …)`:\n"
                + string.Join('\n', offenders));
    }

    private static IEnumerable<string> SourceFiles() =>
        Directory.EnumerateFiles(RepoLayout.SrcDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
                !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    private static List<string> ScanFor(Regex pattern)
    {
        var offenders = new List<string>();
        foreach (var file in SourceFiles())
        {
            var lines = File.ReadAllLines(file);
            var inBlockComment = false;
            for (var i = 0; i < lines.Length; i++)
            {
                // Match against code only -- comments describe the very rule we forbid, so scanning
                // raw text would flag the documentation. String literals are kept.
                var code = StripCommentsPreservingStrings(lines[i], ref inBlockComment);
                if (pattern.IsMatch(code))
                {
                    offenders.Add($"{Path.GetFileName(file)}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        return offenders;
    }

    /// <summary>Return the line with // and /* */ comments removed but string-literal contents kept.</summary>
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
