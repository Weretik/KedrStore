using System.Data.Common;
using BuildingBlocks.Infrastructure.Migrations;
using Catalog.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Platform.Jobs;

/// <summary>Database rules 5.3: migrations wait for a lock at most <see cref="DbMigrator{TContext}.LockTimeout"/>.</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class DbMigratorLockTimeoutTests(CatalogPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Migrate_RunsWithTheLockTimeoutOnItsConnection()
    {
        postgres.SkipIfUnavailable();
        var probe = new LockTimeoutProbe();
        var services = new ServiceCollection();
        services.AddDbContext<CatalogDbContext>(options => options
            .UseNpgsql(postgres.ConnectionString)
            .AddInterceptors(probe));
        await using var provider = services.BuildServiceProvider();

        await new DbMigrator<CatalogDbContext>(provider, NullLogger<DbMigrator<CatalogDbContext>>.Instance)
            .MigrateAsync();

        Assert.Equal(DbMigrator<CatalogDbContext>.LockTimeout, probe.SeenBeforeMigrating);
    }

    /// <summary>Reads lock_timeout on the migration connection just before EF reads the migration history.</summary>
    private sealed class LockTimeoutProbe : DbCommandInterceptor
    {
        public string? SeenBeforeMigrating { get; private set; }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (SeenBeforeMigrating is null && command.CommandText.Contains("__EFMigrationsHistory", StringComparison.Ordinal))
            {
                await using var show = command.Connection!.CreateCommand();
                show.CommandText = "SHOW lock_timeout";
                SeenBeforeMigrating = (string?)await show.ExecuteScalarAsync(cancellationToken);
            }

            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
