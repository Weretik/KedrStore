using BuildingBlocks.Infrastructure.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class SeederExtensions
{
    public static async Task UseAppSeeders(this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeederRunner");


        var allSeeders = services.GetServices<ISeeder>();
        foreach (var seeder in allSeeders.DistinctBy(s => s.GetType()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Виконання: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(services, cancellationToken);
        }
    }
}

