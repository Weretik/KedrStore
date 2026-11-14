using Sales.Infrastructure.Integrations.OneC;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCCounterpartiesJob(OneCCounterpartiesSyncService syncService)
{
    public Task<CounterpartiesSyncResult> RunAsync(CancellationToken cancellationToken)
        => syncService.RunAsync(cancellationToken);
}
