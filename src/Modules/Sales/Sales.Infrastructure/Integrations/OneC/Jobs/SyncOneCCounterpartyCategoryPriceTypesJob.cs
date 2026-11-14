using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Counterparties;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCCounterpartyCategoryPriceTypesJob(OneCCounterpartyCategoryPriceTypesSyncService syncService)
{
    public Task<CounterpartyCategoryPriceTypesSyncResult> RunAsync(CancellationToken cancellationToken)
        => syncService.RunAsync(cancellationToken);
}
