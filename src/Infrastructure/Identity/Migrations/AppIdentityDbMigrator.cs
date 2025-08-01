namespace Infrastructure.Identity.Migrations;

public class AppIdentityDbMigrator: IAppIdentityDbMigrator
{
    public async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var appIdentityDbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppIdentityDbMigrator>>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await appIdentityDbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("✅ Migrations for AppIdentityDbContext applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ AppIdentityDbContext migration error");
            Throw.Application(AppErrors.Database.MigrationFailed
                .WithDetails($"AppIdentityDbContext: {ex.Message}"));
        }
    }
}


