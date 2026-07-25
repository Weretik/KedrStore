using Sales.Infrastructure.Integrations.OneC.Services;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCCounterpartyCategoryPriceTypesJob(OneCCounterpartyCategoryPriceTypesSyncService syncService)
{
    public Task<CounterpartyCategoryPriceTypesSyncResult> RunAsync(CancellationToken cancellationToken)
        => syncService.RunAsync(cancellationToken);
}
