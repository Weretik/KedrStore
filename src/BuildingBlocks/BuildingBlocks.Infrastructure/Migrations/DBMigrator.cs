using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Migrations;

/// <summary>
/// Applies a module's migrations in the pre-deploy migration job. A migration waits at most
/// <see cref="LockTimeout"/> for a table lock and then fails, instead of queueing every query of the running release
/// behind it; the job fails, the deploy stops, and the job is run again (database rules 5.3).
/// A module keeps its migration history in its own schema. The first run after a module moved its history there copies
/// the module's rows from <c>public."__EFMigrationsHistory"</c>, so nothing is applied twice; the old rows stay until a
/// later release, so a rollback still finds them.
/// </summary>
public class DbMigrator<TContext>(IServiceProvider services, ILogger<DbMigrator<TContext>> logger)
    : IDatabaseMigrator
    where TContext : DbContext
{
    public const string LockTimeout = "10s";

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                async ct =>
                {
                    // The session setting holds for every migration because EF keeps an opened connection.
                    await dbContext.Database.OpenConnectionAsync(ct);
                    try
                    {
                        await dbContext.Database.ExecuteSqlRawAsync($"SET lock_timeout = '{LockTimeout}'", ct);
                        await CopyHistoryFromPublicAsync(dbContext, ct);
                        await dbContext.Database.MigrateAsync(ct);
                    }
                    finally
                    {
                        await dbContext.Database.CloseConnectionAsync();
                    }
                },
                cancellationToken);

            logger.LogInformation("✅ Migrations for {Context} applied successfully.", typeof(TContext).Name);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("⏹ Migration for {Context} was canceled.", typeof(TContext).Name);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Migration for {Context} failed with {ExceptionType}.",
                typeof(TContext).Name,
                ex.GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// When the context's history table lives in its own schema and does not exist yet, creates it and copies the
    /// context's own migration rows from the shared <c>public</c> history.
    /// </summary>
    private static async Task CopyHistoryFromPublicAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var relational = RelationalOptionsExtension.Extract(dbContext.GetService<IDbContextOptions>());
        var schema = relational.MigrationsHistoryTableSchema;
        var table = relational.MigrationsHistoryTableName ?? "__EFMigrationsHistory";
        if (string.IsNullOrEmpty(schema) || schema == "public")
            return;

        var mustCopy = await dbContext.Database.SqlQueryRaw<bool>($$"""
            SELECT to_regclass('"{{schema}}"."{{table}}"') IS NULL AND to_regclass('public."{{table}}"') IS NOT NULL AS "Value"
            """).SingleAsync(cancellationToken);
        if (!mustCopy)
            return;

        var migrations = dbContext.Database.GetMigrations().ToArray();
        // Identifiers come from the code, never from input; the migration ids go as a parameter.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync($$"""
            CREATE SCHEMA IF NOT EXISTS "{{schema}}";
            CREATE TABLE "{{schema}}"."{{table}}" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK_{{table}}" PRIMARY KEY ("MigrationId"));
            INSERT INTO "{{schema}}"."{{table}}" ("MigrationId", "ProductVersion")
            SELECT "MigrationId", "ProductVersion" FROM public."{{table}}" WHERE "MigrationId" = ANY({0});
            """, [migrations], cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
