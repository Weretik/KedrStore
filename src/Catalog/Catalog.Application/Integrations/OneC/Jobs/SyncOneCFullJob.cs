namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCFullJob(
    IConfiguration cfg,
    SyncOneCPriceTypesJob priceTypes,
    SyncOneCStocksJob stocks,
    SyncOneCPricesJob prices,
    SyncOneCProductDetailsJob products,
    SyncOneCCategoryJob categories)
{
    [DisableConcurrentExecution(60 * 60 * 3)]
    public async Task RunAsync(IJobCancellationToken jobCancellationToken)
    {
        var doors = Required(cfg, "OneC:DoorsRootCategoryId");
        var hardware = Required(cfg, "OneC:HardwareRootCategoryId");

        await priceTypes.RunAsync(jobCancellationToken);

        await categories.RunAsync(doors, jobCancellationToken);
        await categories.RunAsync(hardware, jobCancellationToken);

        await products.RunAsync(doors, jobCancellationToken);
        await products.RunAsync(hardware, jobCancellationToken);

        await stocks.RunAsync(doors, jobCancellationToken);
        await stocks.RunAsync(hardware, jobCancellationToken);

        await prices.RunAsync(doors, jobCancellationToken);
        await prices.RunAsync(hardware, jobCancellationToken);

    }

    private static string Required(IConfiguration cfg, string key)
        => cfg[key] ?? throw new InvalidOperationException($"Missing config key: {key}");
}
