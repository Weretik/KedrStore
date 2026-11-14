namespace ArchitectureTests.Hosts;

/// <summary>
/// A host is only the composition root: it implements no module port (an Application <c>Abstractions</c> interface
/// or a module <c>Contracts</c> interface), declares no controller and no DbContext.
/// </summary>
public sealed class HostCompositionTests
{
    private static readonly string[] BusinessModules = ["Catalog", "Content", "Identity", "Media", "Sales", "Scheduling", "Work"];

    [Fact]
    public void HostSources_DeclareNoPortImplementationControllerOrDbContext()
    {
        var hosts = Path.Combine(FindRepositoryRoot(), "src", "Bootstrapper");
        var ports = BusinessModules
            .SelectMany(module => new[]
            {
                Path.Combine(FindRepositoryRoot(), "src", "Modules", module, $"{module}.Application", "Abstractions"),
                Path.Combine(FindRepositoryRoot(), "src", "Modules", module, $"{module}.Contracts"),
            })
            .Where(Directory.Exists)
            .SelectMany(path => Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            .Where(path => !IsBuildOutput(path))
            .SelectMany(path => System.Text.RegularExpressions.Regex
                .Matches(File.ReadAllText(path), @"\binterface\s+(I\w+)")
                .Select(match => match.Groups[1].Value))
            .ToHashSet(StringComparer.Ordinal);

        var violations = Directory.EnumerateFiles(hosts, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .SelectMany(path => Declarations(File.ReadAllText(path))
                .Where(bases => bases.Any(ports.Contains) || bases.Contains("ControllerBase") ||
                                bases.Contains("DbContext"))
                .Select(_ => Path.GetRelativePath(hosts, path)))
            .Distinct()
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    private static IEnumerable<string[]> Declarations(string source)
        => System.Text.RegularExpressions.Regex
            .Matches(source, @"\b(?:class|record)\s+\w+(?:<[^>]*>)?(?:\([^)]*\))?\s*:\s*(?<bases>[^{;]+)")
            .Select(match => match.Groups["bases"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(name => name.Split('<', '(')[0].Split('.')[^1])
                .ToArray());

    private static bool IsBuildOutput(string path)
        => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(s => s is "bin" or "obj");

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("KedrStore.sln was not found.");
    }
}
