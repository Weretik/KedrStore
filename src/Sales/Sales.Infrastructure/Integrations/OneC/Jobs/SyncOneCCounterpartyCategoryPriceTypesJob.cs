using Sales.Infrastructure.Integrations.OneC;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCCounterpartyCategoryPriceTypesJob(OneCCounterpartyCategoryPriceTypesSyncService syncService)
{
    public Task<CounterpartyCategoryPriceTypesSyncResult> RunAsync(CancellationToken cancellationToken)
        => syncService.RunAsync(cancellationToken);
}
