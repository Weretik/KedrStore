using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Counterparties;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCCounterpartiesJob(OneCCounterpartiesSyncService syncService)
{
    public Task<CounterpartiesSyncResult> RunAsync(CancellationToken cancellationToken)
        => syncService.RunAsync(cancellationToken);
}
