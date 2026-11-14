using Host.Jobs.Tasks.Catalog;
using Host.Jobs.Tasks.Sales;
using Host.Jobs.Tasks.Site;

namespace Host.Jobs.Tasks;

/// <summary>Every task the tasks job can run, by the keys of the Scheduling catalogue (the outbox runs in the tick).</summary>
public static class ScheduledTaskRunners
{
    public static IReadOnlyList<IScheduledTaskRunner> All { get; } =
    [
        new StocksTaskRunner(),
        new PricesTaskRunner(),
        new CatalogTaskRunner(),
        new CustomersTaskRunner(),
        new OrdersTaskRunner(),
        new MediaTaskRunner(),
        new FullSyncTaskRunner(),
        new ProductListTaskRunner(),
    ];

    public static IScheduledTaskRunner? Find(string key) =>
        All.FirstOrDefault(runner => string.Equals(runner.Key, key, StringComparison.Ordinal));
}
