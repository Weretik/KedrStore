using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Layers;

public sealed class ProjectDependencyTests
{
    private static readonly RepositoryProjectGraph Graph = RepositoryProjectGraph.Load();

    [Fact]
    public void DomainProjects_ReferenceOnlyDomainBuildingBlocks()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Domain", StringComparison.Ordinal))
            .SelectMany(project => project.References
                .Where(reference => reference != "BuildingBlocks.Domain")
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ApplicationProjects_DoNotReferenceApiInfrastructureOrHosts()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Application", StringComparison.Ordinal))
            .SelectMany(project => project.References
                .Where(IsOuterLayerOrHost)
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ApiProjects_DoNotReferenceDomainOrInfrastructure()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Api", StringComparison.Ordinal) &&
                              !project.Name.StartsWith("Host.", StringComparison.Ordinal))
            .SelectMany(project => project.References
                .Where(reference => reference.EndsWith(".Domain", StringComparison.Ordinal) ||
                                    reference.EndsWith(".Infrastructure", StringComparison.Ordinal))
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void InfrastructureProjects_DoNotReferenceApiHostsOrOtherInfrastructure()
    {
        var violations = Graph.Projects
            .Where(project => project.Name.EndsWith(".Infrastructure", StringComparison.Ordinal))
            .SelectMany(project => project.References
                .Where(reference => reference.EndsWith(".Api", StringComparison.Ordinal) ||
                                    reference.StartsWith("Host.", StringComparison.Ordinal) ||
                                    (reference.EndsWith(".Infrastructure", StringComparison.Ordinal) &&
                                     reference != "BuildingBlocks.Infrastructure"))
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void UnitTestProject_HasNoInfrastructureHostOrEfInMemoryDependency()
    {
        var project = Graph.Require("UnitTests");
        var referenceViolations = project.References
            .Where(reference => reference.EndsWith(".Infrastructure", StringComparison.Ordinal) ||
                                reference.StartsWith("Host.", StringComparison.Ordinal))
            .Select(reference => $"UnitTests -> {reference}");
        var packageViolations = project.Packages
            .Where(package => package == "Microsoft.EntityFrameworkCore.InMemory")
            .Select(package => $"UnitTests package -> {package}");

        Assert.Empty(referenceViolations.Concat(packageViolations));
    }

    private static bool IsOuterLayerOrHost(string projectName)
        => projectName.EndsWith(".Api", StringComparison.Ordinal) ||
           projectName.EndsWith(".Infrastructure", StringComparison.Ordinal) ||
           projectName.StartsWith("Host.", StringComparison.Ordinal);
}
