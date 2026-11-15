using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Persistence;

namespace ArchitectureTests.Persistence;

/// <summary>
/// Database rules 3.1: a repository never saves on its own, so every command saves once and its writes commit together.
/// Ardalis's <see cref="RepositoryBase{T}"/> saves inside Add, Update and Delete; module repositories derive from
/// <see cref="UnitOfWorkRepositoryBase{T}"/>, which only records the change.
/// </summary>
public sealed class RepositorySaveTests
{
    private static readonly string[] InfrastructureAssemblies =
        ["Catalog.Infrastructure", "Content.Infrastructure", "Identity.Infrastructure", "Media.Infrastructure",
            "Sales.Infrastructure", "Work.Infrastructure", "Scheduling.Infrastructure"];

    [Fact]
    public void Every_repository_records_changes_without_saving()
    {
        var repositories = InfrastructureAssemblies
            .Select(System.Reflection.Assembly.Load)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } && DerivesFromArdalisRepository(type))
            .ToList();
        var offenders = repositories
            .Where(type => !DerivesFrom(type, typeof(UnitOfWorkRepositoryBase<>)))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(repositories.Count >= 6, $"Expected the module repositories, found {repositories.Count}.");
        Assert.True(offenders.Count == 0,
            "These repositories save inside Add/Update/Delete; derive them from UnitOfWorkRepositoryBase<T>: " +
            string.Join(", ", offenders));
    }

    private static bool DerivesFromArdalisRepository(Type type) => DerivesFrom(type, typeof(RepositoryBase<>));

    private static bool DerivesFrom(Type type, Type openGeneric)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openGeneric)
                return true;
        }

        return false;
    }
}
