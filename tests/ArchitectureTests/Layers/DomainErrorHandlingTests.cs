using System.Text.RegularExpressions;
using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Layers;

/// <summary>
/// A business rule returns <c>DomainResult</c> with a <c>DomainError</c>; an exception in the domain only reports a
/// programming error, such as an argument the caller should never pass.
/// </summary>
public sealed partial class DomainErrorHandlingTests
{
    private static readonly RepositoryProjectGraph Graph = RepositoryProjectGraph.Load();

    private static readonly HashSet<string> ProgrammingErrors =
    [
        "ArgumentException",
        "ArgumentNullException",
        "ArgumentOutOfRangeException",
        "InvalidOperationException",
        "NotSupportedException",
        "UnreachableException",
    ];

    [Fact]
    public void DomainProjects_ThrowOnlyForProgrammingErrors()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Domain", StringComparison.Ordinal))
            .SelectMany(project => Directory.EnumerateFiles(Path.GetDirectoryName(project.Path)!, "*.cs",
                SearchOption.AllDirectories))
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "bin" or "obj"))
            .SelectMany(path => ThrowPattern().Matches(File.ReadAllText(path))
                .Select(match => match.Groups["type"].Value)
                .Where(type => !ProgrammingErrors.Contains(type))
                .Select(type => $"{Path.GetFileName(path)}: throw new {type}"))
            .ToArray();

        Assert.True(violations.Length == 0,
            "Return a DomainResult for a business rule instead of throwing:\n" + string.Join('\n', violations));
    }

    [GeneratedRegex(@"throw\s+new\s+(?:[\w.]+\.)?(?<type>\w+)\s*[(<]")]
    private static partial Regex ThrowPattern();
}
