using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Domain.Entities;

namespace Host.Api.DependencyInjection.WebApplications.Jobs;

public static class SeedJobs
{
    public static async Task AddHangfireSeedJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SeedJobs");
        var jobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        var productRepo = scope.ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();

        logger.LogInformation("[DEBUG_LOG] Checking if products exist for seeding...");
        if (await productRepo.AnyAsync(CancellationToken.None))
        {
            logger.LogInformation("[DEBUG_LOG] Products already exist. Skipping seed job.");
            return;
        }

        logger.LogInformation("[DEBUG_LOG] No products found. Enqueueing SyncOneCFullJob...");
        jobs.Enqueue<SyncOneCFullJob>(
            j => j.RunAsync(JobCancellationToken.Null)
        );
    }
}
