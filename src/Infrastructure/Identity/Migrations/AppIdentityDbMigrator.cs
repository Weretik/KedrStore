using Infrastructure.Identity.Contracts;

namespace Infrastructure.Identity.Migrations;

public class AppIdentityDbMigrator: IAppIdentityDbMigrator
{
    public async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var appIdentityDbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppIdentityDbMigrator>>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var strategy = appIdentityDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                async ct => await appIdentityDbContext.Database.MigrateAsync(ct),
                cancellationToken);


            logger.LogInformation("✅ Migrations for AppIdentityDbContext applied successfully.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("⏹ Migration for {Context} was canceled.", nameof(AppIdentityDbContext));
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ AppIdentityDbContext migration error");
            throw;
        }
    }
}
