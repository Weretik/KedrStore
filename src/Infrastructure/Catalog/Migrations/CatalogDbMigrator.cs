namespace Infrastructure.Catalog.Migrations;

public class CatalogDbMigrator : ICatalogDbMigrator
{
    public async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogDbMigrator>>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var strategy = catalogDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                async ct => await catalogDbContext.Database.MigrateAsync(ct),
                cancellationToken);

            logger.LogInformation("✅ Migrations for CatalogDbContext applied successfully.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("⏹ Migration for {Context} was canceled.", nameof(CatalogDbContext));
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ CatalogDbContext migration error");
            throw;
        }
    }
}
