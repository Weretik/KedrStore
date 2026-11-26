namespace Catalog.Contracts.RetailPrices;

/// <summary>
/// The site's retail price type and how many of its prices the owner's price file keeps from the 1C sync
/// (spec catalog/010 RP-07).
/// </summary>
/// <param name="PriceTypeId">The retail price type, <c>Catalog:Pricing:RetailPriceTypeId</c>.</param>
/// <param name="PriceTypeName">Its name, which is also the price column the file must have.</param>
/// <param name="LockedCount">Retail prices the 1C sync does not change.</param>
/// <param name="LastLockedAt">When a file last locked a price; null when none is locked.</param>
public sealed record RetailPriceStatusDto(int PriceTypeId, string PriceTypeName, int LockedCount,
    DateTimeOffset? LastLockedAt);
