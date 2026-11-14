using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Catalog;

/// <summary>«Залишки з 1С»: stocks of every root, then one rebuild of the product list.</summary>
internal sealed class StocksTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.Stocks;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var stocks = services.GetRequiredService<SyncOneCStocksJob>();
        foreach (var root in OneCRoots.Of(services))
            await stocks.RunAsync(root, cancellationToken, rebuildProjection: false);
        await services.GetRequiredService<IProductListProjectionRebuilder>().RebuildAsync(cancellationToken);
        return "Залишки оновлено.";
    }
}
