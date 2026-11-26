using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.RetailPrices.Shared;
using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Commands.UnlockRetailPrices;

/// <summary>
/// Unlocks the locked retail prices through their root and saves once; only locked prices are loaded, so a repeat
/// finds none and changes nothing. The amounts stay until the next 1C price sync.
/// </summary>
public sealed class UnlockRetailPricesHandler(
    ICatalogRepository<ProductPrice> priceRepo,
    IReadCatalogDbContext db,
    RetailPriceStatusReader status,
    ILogger<UnlockRetailPricesHandler> logger)
    : ICommandHandler<UnlockRetailPricesCommand, Result<RetailPriceStatusDto>>
{
    public async ValueTask<Result<RetailPriceStatusDto>> Handle(UnlockRetailPricesCommand command,
        CancellationToken cancellationToken)
    {
        var retail = status.RetailPriceTypeId;
        // Tracked on purpose: each price is unlocked through its root and saved below.
        var locked = await db.ProductPrices
            .Where(price => price.PriceTypeId == retail && price.LockedFromOneCAt != null)
            .ToListAsync(cancellationToken);
        if (locked.Count > 0)
        {
            foreach (var price in locked)
                price.UnlockFromOneC();
            if (!await RetailPriceSave.TrySaveAsync(priceRepo, cancellationToken))
                return Result<RetailPriceStatusDto>.Conflict(RetailPriceSave.ChangedByOneC);
        }

        logger.LogInformation("Retail prices unlocked from 1C: {Unlocked}", locked.Count);
        return Result.Success(await status.ReadAsync(cancellationToken));
    }
}
