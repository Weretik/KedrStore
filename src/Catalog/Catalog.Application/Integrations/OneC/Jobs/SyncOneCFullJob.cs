namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCFullJob(
    IConfiguration cfg,
    SyncOneCPriceTypesJob priceTypes,
    SyncOneCStocksJob stocks,
    SyncOneCPricesJob prices,
    SyncOneCProductDetailsJob products,
    SyncOneCCategoryJob categories)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var doors = Required(cfg, "OneC:DoorsRootCategoryId");
        var hardware = Required(cfg, "OneC:HardwareRootCategoryId");

        await priceTypes.RunAsync(cancellationToken);

        await categories.RunAsync(doors, cancellationToken);
        await categories.RunAsync(hardware, cancellationToken);

        await products.RunAsync(doors, cancellationToken);
        await products.RunAsync(hardware, cancellationToken);

        await stocks.RunAsync(doors, cancellationToken);
        await stocks.RunAsync(hardware, cancellationToken);

        await prices.RunAsync(doors, cancellationToken);
        await prices.RunAsync(hardware, cancellationToken);
    }

    private static string Required(IConfiguration cfg, string key)
        => cfg[key] ?? throw new InvalidOperationException($"Missing config key: {key}");
}
