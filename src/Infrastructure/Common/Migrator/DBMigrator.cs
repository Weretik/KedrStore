using Infrastructure.Common.Contracts;

namespace Infrastructure.Common.Migrator;

public class DbMigrator<TContext>(IServiceProvider services, ILogger<DbMigrator<TContext>> logger)
    : IDatabaseMigrator
    where TContext : DbContext
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                async ct => await dbContext.Database.MigrateAsync(ct),
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
            logger.LogError(ex, "❌ {Context} migration error", typeof(TContext).Name);
            throw;
        }
    }
}
