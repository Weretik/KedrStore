using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Catalog;

/// <summary>«Ціни з 1С»: price types, prices of every root, then one rebuild of the product list.</summary>
internal sealed class PricesTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.Prices;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        await services.GetRequiredService<SyncOneCPriceTypesJob>().RunAsync(cancellationToken);
        var prices = services.GetRequiredService<SyncOneCPricesJob>();
        foreach (var root in OneCRoots.Of(services))
            await prices.RunAsync(root, cancellationToken, rebuildProjection: false);
        await services.GetRequiredService<IProductListProjectionRebuilder>().RebuildAsync(cancellationToken);
        return "Ціни оновлено.";
    }
}
