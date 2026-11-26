using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Pricing;
using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Shared;

/// <summary>
/// Reads the retail price status without loading aggregates (spec catalog/010 RP-07); the import and unlock answers
/// reuse it.
/// </summary>
public sealed class RetailPriceStatusReader(IReadCatalogDbContext db, IOptions<CatalogPricingOptions> pricing)
{
    public PriceTypeId RetailPriceTypeId => PriceTypeId.Create(pricing.Value.RetailPriceTypeId);

    /// <summary>The retail price type's name, which the file's price column must carry; empty when 1C has not sent it.</summary>
    public async Task<string> ReadNameAsync(CancellationToken cancellationToken)
    {
        var id = RetailPriceTypeId;
        return await db.PriceTypes.AsNoTracking()
            .Where(type => type.Id == id)
            .Select(type => type.PriceTypeName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
    }

    public async Task<RetailPriceStatusDto> ReadAsync(CancellationToken cancellationToken)
    {
        var id = RetailPriceTypeId;
        var locked = db.ProductPrices.AsNoTracking()
            .Where(price => price.PriceTypeId == id && price.LockedFromOneCAt != null);
        var count = await locked.CountAsync(cancellationToken);
        var last = await locked.MaxAsync(price => price.LockedFromOneCAt, cancellationToken);
        return new RetailPriceStatusDto(id.Value, await ReadNameAsync(cancellationToken), count, last);
    }
}
