using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Modules;

/// <summary>
/// Every business module has the same five projects, and a module reaches another one only through that module's
/// <c>*.Contracts</c> project.
/// </summary>
public sealed class ModuleContractsTests
{
    private static readonly RepositoryProjectGraph Graph = RepositoryProjectGraph.Load();
    private static readonly string[] BusinessModules = ["Catalog", "Content", "Identity", "Media", "Sales", "Work"];
    private static readonly string[] Layers = ["Api", "Application", "Contracts", "Domain", "Infrastructure"];

    public static TheoryData<string> Modules => new(BusinessModules);

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_HasApiApplicationContractsDomainAndInfrastructure(string module)
    {
        var missing = Layers.Where(layer => !Graph.Projects.Any(p => p.Name == $"{module}.{layer}")).ToArray();

        Assert.True(missing.Length == 0, $"{module} is missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void ModuleProjects_ReachAnotherModuleOnlyThroughItsContracts()
    {
        var violations = Graph.Projects
            .Select(project => (Project: project, Owner: OwnerOf(project.Name)))
            .Where(item => item.Owner is not null)
            .SelectMany(item => item.Project.References
                .Where(reference => OwnerOf(reference) is { } other && other != item.Owner &&
                                    !reference.EndsWith(".Contracts", StringComparison.Ordinal))
                .Select(reference => $"{item.Project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ContractsProjects_DependOnNoOtherProject()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Contracts", StringComparison.Ordinal))
            .SelectMany(project => project.References.Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    private static string? OwnerOf(string projectName)
        => BusinessModules.SingleOrDefault(module => projectName.StartsWith($"{module}.", StringComparison.Ordinal));
}
