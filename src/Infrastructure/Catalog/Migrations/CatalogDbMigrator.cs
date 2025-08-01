namespace Infrastructure.Catalog.Migrations;

public class CatalogDbMigrator : ICatalogDbMigrator
{
    public async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogDbMigrator>>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await catalogDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("✅ Migrations for CatalogDbContext applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ CatalogDbContext migration error");
            Throw.Application(AppErrors.Database.MigrationFailed
                .WithDetails($"CatalogDbContext: {ex.Message}"));
        }
    }
}
