using Domain.Catalog.Entities;
// ReSharper disable All

namespace Application.Catalog.GetProducts;

public static class ProductFiltersExtension
{
    public static ISpecificationBuilder<Product> ApplyFilters(
        this ISpecificationBuilder<Product> specification,
        ProductFilter filter,
        PricingOptions pricingOptions)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(pricingOptions);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm!.Trim();
            var tokens = term.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
            {
                specification.Where(p => p.Id == id);
            }
            else
            {
                foreach (var raw in tokens)
                {
                    var text = EscapeLike(raw);
                    var pattern = $"%{text}%";
                    specification.Where(
                        p => EF.Functions.ILike(p.Name, pattern, @"\"));
                }
            }
        }

        if (filter.CategoryId is not null)
            specification.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (pricingOptions.MinPrice.HasValue)
            specification.Where(p =>
                p.Prices.Any(pp =>
                    pp.PriceType.Name == pricingOptions.PriceType &&
                    pp.Amount >= pricingOptions.MinPrice.Value));

        if (pricingOptions.MaxPrice.HasValue)
            specification.Where(p =>
                p.Prices.Any(pp =>
                    pp.PriceType.Name == pricingOptions.PriceType &&
                    pp.Amount <= pricingOptions.MaxPrice.Value));

        if (filter.Stock.HasValue)
            specification.Where(p => p.Stock == filter.Stock.Value);

        return specification;
    }

    private static string EscapeLike(string text)
    {
        return text
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }
}
