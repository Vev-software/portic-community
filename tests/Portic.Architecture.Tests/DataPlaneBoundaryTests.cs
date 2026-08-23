using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Portic.Architecture.Tests;

/// <summary>
/// FITNESS TEST -- enforces the data-plane rule from portic-community#14: this runtime must not grow
/// a control-plane database dependency on the gateway request path.
/// </summary>
public sealed class DataPlaneBoundaryTests
{
    private static readonly IReadOnlyList<string> DatabasePackageMarkers =
    [
        "EntityFrameworkCore",
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient",
        "Npgsql",
        "MySqlConnector",
        "Pomelo.EntityFrameworkCore.MySql",
        "MongoDB.Driver",
        "RavenDB.Client",
        "StackExchange.Redis",
        "Dapper",
    ];

    private static readonly Regex DirectDatabaseApi = new(
        @"\b(DbContext|DbConnection|IDbConnection|SqlConnection|NpgsqlConnection|MySqlConnection|MongoClient|ConnectionMultiplexer)\b",
        RegexOptions.Compiled);

    [Fact]
    public void Runtime_projects_do_not_reference_database_or_control_plane_client_packages()
    {
        var violations = new List<string>();

        foreach (var projectPath in RepoLayout.ProjectFiles())
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            foreach (var packageId in PackageReferences(projectPath))
            {
                if (LooksLikeDatabasePackage(packageId))
                {
                    violations.Add($"{projectName} references database/control-plane package '{packageId}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "The gateway data plane must not depend on a control-plane database/client package:\n  "
                + string.Join("\n  ", violations));
    }

    [Fact]
    public void Runtime_source_does_not_use_direct_database_apis()
    {
        var offenders = new List<string>();

        foreach (var file in SourceFiles())
        {
            var lines = File.ReadAllLines(file);
            var inBlockComment = false;
            for (var i = 0; i < lines.Length; i++)
            {
                var code = StripCommentsPreservingStrings(lines[i], ref inBlockComment);
                if (DirectDatabaseApi.IsMatch(code))
                {
                    offenders.Add($"{Path.GetRelativePath(RepoLayout.RepoRoot, file)}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "The gateway data plane must not call database APIs directly:\n  "
                + string.Join("\n  ", offenders));
    }

    private static bool LooksLikeDatabasePackage(string packageId) =>
        DatabasePackageMarkers.Any(marker => packageId.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> PackageReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        return document.Descendants()
            .Where(e => e.Name.LocalName == "PackageReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!);
    }

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
