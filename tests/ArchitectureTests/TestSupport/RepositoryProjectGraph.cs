using System.Xml.Linq;

namespace ArchitectureTests.TestSupport;

internal sealed class RepositoryProjectGraph
{
    private readonly IReadOnlyDictionary<string, ProjectNode> _projects;

    private RepositoryProjectGraph(IReadOnlyDictionary<string, ProjectNode> projects)
        => _projects = projects;

    public IReadOnlyCollection<ProjectNode> Projects => _projects.Values.ToArray();

    public ProjectNode Require(string name)
        => _projects.TryGetValue(name, out var project)
            ? project
            : throw new InvalidOperationException($"Project '{name}' was not found in the repository graph.");

    public static RepositoryProjectGraph Load()
    {
        var root = FindRepositoryRoot();
        var projectPaths = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !HasBuildOutputSegment(path))
            .ToArray();
        var knownPaths = projectPaths.ToDictionary(Path.GetFullPath, StringComparer.OrdinalIgnoreCase);
        var projects = new Dictionary<string, ProjectNode>(StringComparer.OrdinalIgnoreCase);

        foreach (var projectPath in projectPaths)
        {
            var document = XDocument.Load(projectPath);
            var references = document.Descendants("ProjectReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => NormalizeProjectReferencePath(projectPath, value!))
                .Select(path => knownPaths.TryGetValue(path, out var canonicalPath) ? canonicalPath : path)
                .Select(path => Path.GetFileNameWithoutExtension(path)!)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var packages = document.Descendants("PackageReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var name = Path.GetFileNameWithoutExtension(projectPath);
            projects.Add(name, new ProjectNode(name, projectPath, references, packages));
        }

        return new RepositoryProjectGraph(projects);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("KedrStore.sln was not found above the test output directory.");
    }

    private static bool HasBuildOutputSegment(string path)
        => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");

    private static string NormalizeProjectReferencePath(string projectPath, string reference)
    {
        var normalizedReference = reference
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath)!, normalizedReference));
    }
}

internal sealed record ProjectNode(
    string Name,
    string Path,
    IReadOnlyCollection<string> References,
    IReadOnlyCollection<string> Packages);
