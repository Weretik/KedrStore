using Catalog.Application.Contracts.Projections;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCFullJob(
    IConfiguration сonfiguration,
    SyncOneCPriceTypesJob priceTypes,
    SyncOneCStocksJob stocks,
    SyncOneCPricesJob prices,
    SyncOneCProductDetailsJob products,
    SyncOneCCategoryJob categories,
    IProductListProjectionRebuilder productListProjectionRebuilder)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var doors = Required(сonfiguration, "OneC:DoorsRootCategoryId");
        var hardware = Required(сonfiguration, "OneC:HardwareRootCategoryId");
        var cosmos = Required(сonfiguration, "OneC:CosmosRootCategoryId");

        await priceTypes.RunAsync(cancellationToken);

        await categories.RunAsync(doors, cancellationToken);
        await categories.RunAsync(hardware, cancellationToken);
        await categories.RunAsync(cosmos, cancellationToken);

        await products.RunAsync(doors, cancellationToken, rebuildProjection: false);
        await products.RunAsync(hardware, cancellationToken, rebuildProjection: false);
        await products.RunAsync(cosmos, cancellationToken, rebuildProjection: false);

        await stocks.RunAsync(doors, cancellationToken, rebuildProjection: false);
        await stocks.RunAsync(hardware, cancellationToken, rebuildProjection: false);
        await stocks.RunAsync(cosmos, cancellationToken, rebuildProjection: false);

        await prices.RunAsync(doors, cancellationToken, rebuildProjection: false);
        await prices.RunAsync(hardware, cancellationToken, rebuildProjection: false);
        await prices.RunAsync(cosmos, cancellationToken, rebuildProjection: false);

        await productListProjectionRebuilder.RebuildAsync(cancellationToken);
    }

    private static string Required(IConfiguration cfg, string key)
        => cfg[key] ?? throw new InvalidOperationException($"Missing config key: {key}");
}
