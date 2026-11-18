using BuildingBlocks.Infrastructure.Migrations;
using Catalog.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace IntegrationTests.Platform.Jobs;

/// <summary>
/// Database rules 5.1: a module that moves its migration history from <c>public</c> into its own schema keeps every
/// applied migration; the migrator copies only its own rows, applies nothing twice, and a repeat run changes nothing.
/// Runs on its own database.
/// </summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class DbMigratorHistoryMoveTests(CatalogPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Migrate_CopiesTheModuleHistoryFromPublicOnce()
    {
        postgres.SkipIfUnavailable();
        var database = "kedr_history_move_" + Guid.NewGuid().ToString("N")[..8];
        await ExecuteAsync(postgres.ConnectionString, $"CREATE DATABASE {database}");
        var connectionString = new NpgsqlConnectionStringBuilder(postgres.ConnectionString) { Database = database }.ConnectionString;
        try
        {
            string[] all;
            await using (var old = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connectionString).Options))
            {
                await old.Database.MigrateAsync();
                all = old.Database.GetMigrations().ToArray();
                await old.Database.ExecuteSqlRawAsync("""
                    INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20200101000000_OtherModule', '10.0.0')
                    """);
            }

            var services = new ServiceCollection();
            services.AddDbContext<CatalogDbContext>(options => options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", CatalogDbContext.Schema)));
            await using var provider = services.BuildServiceProvider();
            var migrator = new DbMigrator<CatalogDbContext>(provider, NullLogger<DbMigrator<CatalogDbContext>>.Instance);

            await migrator.MigrateAsync();
            await migrator.MigrateAsync();

            await using var scope = provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            Assert.Equal(all, await db.Database.GetAppliedMigrationsAsync());
            Assert.Equal(all.Length, await CountAsync(db, "catalog"));
            Assert.Equal(all.Length + 1, await CountAsync(db, "public"));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(postgres.ConnectionString, $"DROP DATABASE IF EXISTS {database} WITH (FORCE)");
        }
    }

    private static Task<int> CountAsync(CatalogDbContext db, string schema)
        => db.Database.SqlQueryRaw<int>($"SELECT COUNT(*)::integer AS \"Value\" FROM {schema}.\"__EFMigrationsHistory\"").SingleAsync();

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
