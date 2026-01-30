using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Persistence;
using Catalog.Domain.Entities;

namespace Host.Api.DependencyInjection.WebApplications.HostPipelines;

public static class SeedJobs
{
    public static async Task AddHangfireSeedJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        var productRepo = scope.ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();

        if (await productRepo.AnyAsync(CancellationToken.None))
            return;

        jobs.Enqueue<SyncOneCFullJob>(
            j => j.RunAsync(JobCancellationToken.Null)
        );
    }
}
