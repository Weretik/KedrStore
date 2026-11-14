using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Orders;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCOrdersJob(SyncOneCOrdersService syncService)
{
    public Task<SyncOneCOrdersResult> RunAsync(CancellationToken cancellationToken)
        => syncService.RunAsync(cancellationToken);
}
