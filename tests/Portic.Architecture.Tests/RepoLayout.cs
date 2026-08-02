namespace Portic.Architecture.Tests;

/// <summary>
/// Locates the repository root (the directory containing Portic.sln) starting from the test binary,
/// so the fitness tests can scan source and build outputs regardless of working directory.
/// </summary>
internal static class RepoLayout
{
    public static string RepoRoot { get; } = FindRepoRoot();

    public static string SrcDirectory => Path.Combine(RepoRoot, "src");

    public static IReadOnlyList<string> ProjectFiles() =>
        Directory.EnumerateFiles(SrcDirectory, "*.csproj", SearchOption.AllDirectories).ToArray();

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Portic.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate Portic.sln above '{AppContext.BaseDirectory}'.");
    }
}
