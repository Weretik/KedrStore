using Catalog.Infrastructure.Persistence;
using Content.Infrastructure.Persistence;
using Identity.Infrastructure.DataBase;
using Media.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sales.Infrastructure.DataBase;
using Work.Infrastructure.Persistence;

namespace ArchitectureTests.Modules;

/// <summary>
/// A module's data model links to another module only by an id value: no entity of another module in its
/// <c>DbContext</c>, no navigation property to one, and no foreign key into another module's table. What the
/// aggregate needs from the other module (a name, a price at that moment) is kept as a snapshot in its own columns.
/// </summary>
public sealed class ModuleDataIsolationTests
{
    private static readonly string[] SharedNamespaces = ["BuildingBlocks", "Microsoft", "System"];

    public static TheoryData<string> Contexts => new() { "Catalog", "Content", "Identity", "Media", "Sales", "Work" };

    [Theory]
    [MemberData(nameof(Contexts))]
    public void ModelHoldsOnlyItsOwnEntities_AndLinksOnlyInsideTheModule(string module)
    {
        using var context = Create(module);
        var violations = new List<string>();

        foreach (var entity in context.Model.GetEntityTypes())
        {
            if (!BelongsTo(entity.ClrType, module))
                violations.Add($"{entity.DisplayName()}: an entity of another module is mapped");

            violations.AddRange(entity.GetNavigations().Cast<INavigationBase>().Concat(entity.GetSkipNavigations())
                .Where(navigation => !BelongsTo(navigation.TargetEntityType.ClrType, module))
                .Select(navigation => $"{entity.DisplayName()}.{navigation.Name}: navigation to another module"));

            violations.AddRange(entity.GetForeignKeys()
                .Where(key => !BelongsTo(key.PrincipalEntityType.ClrType, module)
                              || key.PrincipalEntityType.GetSchema() != entity.GetSchema())
                .Select(key => $"{entity.DisplayName()}: foreign key to {key.PrincipalEntityType.DisplayName()} " +
                               $"in schema {key.PrincipalEntityType.GetSchema() ?? "(default)"}"));
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    private static bool BelongsTo(Type type, string module)
    {
        var root = type.Namespace?.Split('.')[0];
        return root == module || SharedNamespaces.Contains(root);
    }

    private static DbContext Create(string module) => module switch
    {
        "Catalog" => new CatalogDbContext(Options<CatalogDbContext>()),
        "Content" => new ContentDbContext(Options<ContentDbContext>()),
        "Identity" => new AppIdentityDbContext(Options<AppIdentityDbContext>()),
        "Media" => new MediaDbContext(Options<MediaDbContext>()),
        "Sales" => new SalesDbContext(Options<SalesDbContext>()),
        "Work" => new WorkDbContext(Options<WorkDbContext>()),
        _ => throw new ArgumentOutOfRangeException(nameof(module), module, null),
    };

    // Building the model does not open a connection.
    private static DbContextOptions<T> Options<T>() where T : DbContext
        => new DbContextOptionsBuilder<T>().UseNpgsql("Host=localhost;Database=model-only").Options;
}
