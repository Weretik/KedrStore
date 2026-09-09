using System.Text.RegularExpressions;
using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Modules;

public sealed partial class TestOrganizationTests
{
    private static readonly RepositoryProjectGraph Graph = RepositoryProjectGraph.Load();
    private static readonly string[] TestProjects = ["UnitTests", "IntegrationTests", "ArchitectureTests"];

    [Fact]
    public void TestSources_AreGroupedByResponsibilityAndNamespaceMatchesPath()
    {
        var violations = new List<string>();

        foreach (var projectName in TestProjects)
        {
            var projectDirectory = Path.GetDirectoryName(Graph.Require(projectName).Path)!;
            var sources = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(path => !HasBuildOutputSegment(path));

            foreach (var source in sources)
            {
                var relativePath = Path.GetRelativePath(projectDirectory, source);
                var directory = Path.GetDirectoryName(relativePath);
                if (string.IsNullOrEmpty(directory))
                {
                    violations.Add($"{relativePath}: test source must be inside a responsibility directory");
                    continue;
                }

                var expectedNamespace = $"{projectName}.{directory.Replace(Path.DirectorySeparatorChar, '.')}";
                var sourceText = File.ReadAllText(source);
                var namespaceMatch = FileScopedNamespace().Match(sourceText);
                if (!namespaceMatch.Success || namespaceMatch.Groups[1].Value != expectedNamespace)
                {
                    violations.Add(
                        $"{relativePath}: expected namespace '{expectedNamespace}', found '{namespaceMatch.Groups[1].Value}'");
                }
            }
        }

        Assert.Empty(violations);
    }

    private static bool HasBuildOutputSegment(string path)
        => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");

    [GeneratedRegex(@"\bnamespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*;")]
    private static partial Regex FileScopedNamespace();
}
