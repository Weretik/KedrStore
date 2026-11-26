namespace Catalog.Contracts.RetailPrices;

/// <summary>What an uploaded price file changed (spec catalog/010 RP-08).</summary>
/// <param name="Changed">Retail prices added or with a new amount.</param>
/// <param name="Unchanged">Retail prices that already had the file's amount.</param>
/// <param name="NotFoundCount">Rows whose code is not a product in the catalog.</param>
/// <param name="NotFoundCodes">The first 50 of those codes, in file order.</param>
/// <param name="WithoutPriceCount">Rows of products without a usable price (empty, not a number, zero, or too large).</param>
/// <param name="WithoutPriceCodes">The first 50 of those codes, in file order.</param>
/// <param name="Status">The retail price status after the upload.</param>
public sealed record RetailPriceImportResultDto(int Changed, int Unchanged, int NotFoundCount,
    IReadOnlyList<string> NotFoundCodes, int WithoutPriceCount, IReadOnlyList<string> WithoutPriceCodes,
    RetailPriceStatusDto Status);
