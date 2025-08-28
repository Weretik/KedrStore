namespace Infrastructure.Extensions;

public static class SeederExtensions
{
    public static async Task UseAppSeeders(this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeederRunner");


        // Универсальный запуск всех сидеров
        var allSeeders = services.GetServices<ISeeder>();
        foreach (var seeder in allSeeders.DistinctBy(s => s.GetType()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Виконання: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(services, cancellationToken);
        }
    }

    public static async Task UseIdentitySeeders(this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var seeders = services.GetServices<IIdentitySeeder>();
        foreach (var seeder in seeders.DistinctBy(s => s.GetType()))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await seeder.SeedAsync(services, cancellationToken);
        }
    }

    public static async Task UseCatalogSeeders(this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var seeders = services.GetServices<ICatalogSeeder>();
        foreach (var seeder in seeders.DistinctBy(s => s.GetType()))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await seeder.SeedAsync(services, cancellationToken);
        }
    }
}

