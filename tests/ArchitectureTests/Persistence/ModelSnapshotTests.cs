using Catalog.Infrastructure.Persistence;
using Content.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Media.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Sales.Infrastructure.Persistence;
using Work.Infrastructure.Persistence;

namespace ArchitectureTests.Persistence;

/// <summary>
/// Moving entity or configuration classes must not change a module's EF model:
/// each DbContext model has to match its latest migration snapshot. No database is needed.
/// </summary>
public sealed class ModelSnapshotTests
{
    private const string ConnectionString = "Host=localhost;Database=model_check;Username=model_check;Password=not-used";

    public static TheoryData<string> Contexts => new()
    {
        nameof(AppIdentityDbContext),
        nameof(CatalogDbContext),
        nameof(ContentDbContext),
        nameof(MediaDbContext),
        nameof(SalesDbContext),
        nameof(WorkDbContext),
    };

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Model_matches_latest_migration_snapshot(string contextName)
    {
        using var context = Create(contextName);

        Assert.False(
            context.Database.HasPendingModelChanges(),
            $"{contextName} model differs from its migration snapshot: add a migration or undo the mapping change.");
    }

    private static DbContext Create(string contextName) => contextName switch
    {
        nameof(AppIdentityDbContext) => new AppIdentityDbContext(Plain<AppIdentityDbContext>()),
        nameof(CatalogDbContext) => new CatalogDbContext(Plain<CatalogDbContext>()),
        nameof(SalesDbContext) => new SalesDbContext(Plain<SalesDbContext>()),
        nameof(ContentDbContext) => new ContentDbContext(Configured<ContentDbContext>(ContentDbContext.Configure)),
        nameof(MediaDbContext) => new MediaDbContext(Configured<MediaDbContext>(MediaDbContext.Configure)),
        nameof(WorkDbContext) => new WorkDbContext(Configured<WorkDbContext>(WorkDbContext.Configure)),
        _ => throw new ArgumentOutOfRangeException(nameof(contextName), contextName, null),
    };

    private static DbContextOptions<T> Plain<T>() where T : DbContext =>
        new DbContextOptionsBuilder<T>().UseNpgsql(ConnectionString).Options;

    private static DbContextOptions<T> Configured<T>(Action<DbContextOptionsBuilder, string> configure)
        where T : DbContext
    {
        var builder = new DbContextOptionsBuilder<T>();
        configure(builder, ConnectionString);
        return builder.Options;
    }
}
