using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Migrations;

/// <summary>
/// Applies a module's migrations in the pre-deploy migration job. A migration waits at most
/// <see cref="LockTimeout"/> for a table lock and then fails, instead of queueing every query of the running release
/// behind it; the job fails, the deploy stops, and the job is run again (database rules 5.3).
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
}
