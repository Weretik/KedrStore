using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static async Task UseAppMigrations(
        this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var migrators = services.GetServices<IDatabaseMigrator>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeederRunner");

        logger.LogInformation("🚀 Launch migrations for all contexts...");

        foreach (var migrator in migrators)
        {
            var name = migrator.GetType().Name;

            logger.LogInformation("➡️ Migrating {MigratorName}...", name);

            cancellationToken.ThrowIfCancellationRequested();
            await migrator.MigrateAsync(cancellationToken);

            logger.LogInformation("✅ {MigratorName} migration completed", name);
        }

        logger.LogInformation("✅ All migrations were successful..");
    }
}
