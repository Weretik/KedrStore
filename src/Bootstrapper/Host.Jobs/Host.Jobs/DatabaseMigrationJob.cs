using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Microsoft.Extensions.Logging;

namespace Host.Jobs;

public sealed class DatabaseMigrationJob(
    IEnumerable<IDatabaseMigrator> migrators,
    IEnumerable<ISeeder> seeders,
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var orderedMigrators = migrators
            .DistinctBy(migrator => migrator.GetType())
            .OrderBy(GetMigratorOrder)
            .ThenBy(migrator => migrator.GetType().FullName, StringComparer.Ordinal)
            .ToArray();

        foreach (var migrator in orderedMigrators)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var contextName = GetContextName(migrator);

            logger.LogInformation("Starting database migration for {DbContext}", contextName);
            await migrator.MigrateAsync(cancellationToken);
            logger.LogInformation("Completed database migration for {DbContext}", contextName);
        }

        foreach (var seeder in seeders.DistinctBy(seeder => seeder.GetType()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Starting database seed for {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(serviceProvider, cancellationToken);
            logger.LogInformation("Completed database seed for {Seeder}", seeder.GetType().Name);
        }
    }

    private static int GetMigratorOrder(IDatabaseMigrator migrator)
        => GetContextName(migrator) switch
        {
            "CatalogDbContext" => 10,
            "SalesDbContext" => 20,
            "AppIdentityDbContext" => 30,
            _ => 100
        };

    private static string GetContextName(IDatabaseMigrator migrator)
        => migrator.GetType().IsGenericType
            ? migrator.GetType().GetGenericArguments()[0].Name
            : migrator.GetType().Name;
}
