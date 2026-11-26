using Catalog.Application.Abstractions.Files;

namespace Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;

/// <summary>What the file's rows mean (spec catalog/010 RP-02, RP-03): prices by product, and codes to report.</summary>
public sealed record RetailPriceFilePlan(
    IReadOnlyDictionary<ProductId, decimal> Prices,
    IReadOnlyList<string> NotFoundCodes,
    IReadOnlyList<string> WithoutPriceCodes);

/// <summary>Pure rules for the rows of the owner's price file.</summary>
public static class RetailPriceFileRows
{
    /// <summary>The product codes in the file, to look up in the catalog in one query.</summary>
    public static IReadOnlyList<int> Codes(IEnumerable<RetailPriceSheetRow> rows)
        => rows.Select(row => ParseCode(row.Code)).OfType<int>().Distinct().ToList();

    /// <summary>
    /// A row without a code is a title or group and is ignored; a code that is not a product is reported as not found;
    /// a product without a price above zero is reported as without price; when a code repeats, its last price wins.
    /// </summary>
    public static RetailPriceFilePlan Plan(IEnumerable<RetailPriceSheetRow> rows, IReadOnlySet<int> products)
    {
        var prices = new Dictionary<ProductId, decimal>();
        var notFound = new List<string>();
        var withoutPrice = new List<string>();
        foreach (var row in rows)
        {
            var text = row.Code.Trim();
            if (text.Length == 0)
                continue;

            if (ParseCode(text) is not { } code || !products.Contains(code))
                notFound.Add(text);
            else if (row.Price is not { } price || price <= 0m)
                withoutPrice.Add(text);
            else
                prices[ProductId.Create(code)] = price;
        }

        return new RetailPriceFilePlan(prices, notFound, withoutPrice);
    }

    private static int? ParseCode(string code)
        => int.TryParse(code.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : null;
}
