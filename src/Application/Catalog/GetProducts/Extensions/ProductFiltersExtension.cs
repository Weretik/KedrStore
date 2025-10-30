using Domain.Catalog.Entities;

namespace Application.Catalog.GetProducts;

public static class ProductFiltersExtension
{
    public static ISpecificationBuilder<Product> ApplyFilters(this ISpecificationBuilder<Product> specification,
        ProductFilter filter, int priceTypeId)
    {
        ArgumentNullException.ThrowIfNull(filter);

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
            specification.Where(p => p.CategoryId == filter.CategoryId);

        if (filter.MinPrice.HasValue)
            specification.Where(p =>
                p.Prices.Any(pp =>
                    pp.PriceType == priceTypeId &&
                    pp.Amount >= filter.MinPrice.Value));

        if (filter.MaxPrice.HasValue)
            specification.Where(p =>
                p.Prices.Any(pp =>
                    pp.PriceType == priceTypeId &&
                    pp.Amount <= filter.MaxPrice.Value));

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
