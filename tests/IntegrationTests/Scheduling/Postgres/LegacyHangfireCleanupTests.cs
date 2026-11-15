using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IntegrationTests.Scheduling.Postgres;

/// <summary>The Scheduling migration that removes the tables Hangfire left in production databases.</summary>
[Collection(SchedulingPostgresCollection.Name)]
public sealed class LegacyHangfireCleanupTests(SchedulingPostgresFixture postgres)
{
    private const string BeforeCleanup = "20261114174952_InitialScheduling";

    [SkippableFact]
    public async Task Migration_drops_the_hangfire_tables_and_schema_and_runs_again_on_a_database_without_them()
    {
        postgres.SkipIfUnavailable();
        await using var db = postgres.CreateContext();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync(BeforeCleanup);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE SCHEMA IF NOT EXISTS hangfire;
            CREATE TABLE hangfire."job" (id bigserial PRIMARY KEY, statename text);
            CREATE TABLE hangfire."state" (id bigserial PRIMARY KEY, jobid bigint REFERENCES hangfire."job" (id));
            CREATE TABLE hangfire."lock" (resource text PRIMARY KEY);
            """);

        await migrator.MigrateAsync();
        await migrator.MigrateAsync(BeforeCleanup);
        await migrator.MigrateAsync();

        Assert.False(await db.Database
            .SqlQueryRaw<bool>("""SELECT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'hangfire') AS "Value" """)
            .SingleAsync());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }
}
