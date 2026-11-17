using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Modules;

/// <summary>
/// Every business module has the same five projects, and a module reaches another one only through that module's
/// <c>*.Contracts</c> project; module references never form a cycle, so the context map reads one way.
/// </summary>
public sealed class ModuleContractsTests
{
    private static readonly RepositoryProjectGraph Graph = RepositoryProjectGraph.Load();
    private static readonly string[] BusinessModules = ["Catalog", "Content", "Identity", "Media", "Sales", "Scheduling", "Work"];
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
    public void BusinessModules_ListEveryFolderUnderSrcModules()
    {
        var onDisk = Directory.GetDirectories(Path.Combine(RepositoryRoot.Find(), "src", "Modules"))
            .Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(onDisk, BusinessModules.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void ModuleReferences_FormNoCycle()
    {
        var uses = BusinessModules.ToDictionary(module => module, module => Graph.Projects
            .Where(project => OwnerOf(project.Name) == module)
            .SelectMany(project => project.References)
            .Select(OwnerOf)
            .OfType<string>()
            .Where(other => other != module)
            .ToHashSet(StringComparer.Ordinal));

        var cycles = uses.SelectMany(pair => pair.Value
                .Where(other => Reaches(uses, other, pair.Key, []))
                .Select(other => $"{pair.Key} -> {other} -> ... -> {pair.Key}"))
            .ToArray();

        Assert.Empty(cycles);
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

    private static bool Reaches(Dictionary<string, HashSet<string>> uses, string from, string target, HashSet<string> seen)
        => from == target || (seen.Add(from) && uses[from].Any(next => Reaches(uses, next, target, seen)));

    private static string? OwnerOf(string projectName)
        => BusinessModules.SingleOrDefault(module => projectName.StartsWith($"{module}.", StringComparison.Ordinal));
}
