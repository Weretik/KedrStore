using Catalog.Application.Features.ProductMedia.Jobs;
using Media.Application.Features.Files.Jobs.SyncCdnMedia;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Site;

/// <summary>«Фото з CDN»: register new CDN files, link them to products, lay them out by category (as media-cdn-sync).</summary>
internal sealed class MediaTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.Media;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var sync = await services.GetRequiredService<SyncCdnMediaJob>().RunAsync(false, cancellationToken);
        var links = await services.GetRequiredService<LinkProductMediaJob>().RunAsync(false, cancellationToken);
        var folders = await services.GetRequiredService<ArrangeProductMediaFoldersJob>().RunAsync(false, cancellationToken);
        return $"Нових фото {sync.Registered}; привʼязано до товарів {links.PhotosLinked + links.SchemesLinked}; " +
               $"переміщено {folders.FilesMoved}; товарів без фото {links.ProductsWithoutPhoto}.";
    }
}
