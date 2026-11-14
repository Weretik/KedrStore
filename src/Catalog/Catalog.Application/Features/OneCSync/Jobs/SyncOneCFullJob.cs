using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Jobs;

namespace Catalog.Application.Features.OneCSync.Jobs;

public sealed class SyncOneCFullJob(
    IConfiguration сonfiguration,
    SyncOneCPriceTypesJob priceTypes,
    SyncOneCStocksJob stocks,
    SyncOneCPricesJob prices,
    SyncOneCProductDetailsJob products,
    SyncOneCCategoryJob categories,
    RecognizeCatalogAttributesJob attributes,
    IProductListProjectionRebuilder productListProjectionRebuilder,
    ILogger<SyncOneCFullJob> logger)
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

        await RecognizeAttributesAsync(cancellationToken);

        await productListProjectionRebuilder.RebuildAsync(cancellationToken);
    }

    /// <summary>
    /// New and renamed products get attributes from their names (spec catalog/006, phase 4). A failure is logged and
    /// does not stop prices, stocks, or the list rebuild.
    /// </summary>
    private async Task RecognizeAttributesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var report = await attributes.RunAsync(apply: true, productIds: null, cancellationToken);
            if (report.Errors.Count > 0)
                logger.LogWarning("Attribute recognition after the 1C sync reported {Errors} errors; first: {First}",
                    report.Errors.Count, report.Errors[0]);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Attribute recognition after the 1C sync failed.");
        }
    }

    private static string Required(IConfiguration cfg, string key)
        => cfg[key] ?? throw new InvalidOperationException($"Missing config key: {key}");
}
