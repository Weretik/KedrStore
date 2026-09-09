using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Modules;

public sealed class ProjectModuleIsolationTests
{
    private static readonly RepositoryProjectGraph Graph = RepositoryProjectGraph.Load();
    private static readonly string[] BusinessModules = ["Catalog", "Identity", "Sales"];

    [Fact]
    public void DomainAndApplicationProjects_DoNotReferenceAnotherBusinessModule()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Domain", StringComparison.Ordinal) ||
                              project.Name.EndsWith(".Application", StringComparison.Ordinal))
            .SelectMany(project => project.References
                .Where(reference => IsOtherBusinessModule(project.Name, reference))
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void BuildingBlocksProjects_DoNotReferenceBusinessModules()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.StartsWith("BuildingBlocks.", StringComparison.Ordinal))
            .SelectMany(project => project.References
                .Where(reference => BusinessModules.Any(module =>
                    reference.StartsWith($"{module}.", StringComparison.Ordinal)))
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    private static bool IsOtherBusinessModule(string projectName, string reference)
    {
        var owner = BusinessModules.SingleOrDefault(module =>
            projectName.StartsWith($"{module}.", StringComparison.Ordinal));
        if (owner is null)
            return false;

        return BusinessModules
            .Where(module => module != owner)
            .Any(module => reference.StartsWith($"{module}.", StringComparison.Ordinal));
    }
}
