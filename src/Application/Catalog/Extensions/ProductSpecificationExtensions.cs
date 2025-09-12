namespace Application.Catalog.Extensions;

public static class ProductFiltersSpecificationExtensions
{
    public static ISpecificationBuilder<Product> ApplyCommonFilters(
        this ISpecificationBuilder<Product> specification,
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var tokens = term.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                && id > 0)
            {
                specification.Where(p => p.Id == id);
            }
            else
            {
                foreach (var raw in tokens)
                {
                    var text = EscapeLike(raw);
                    var pattern = $"%{text}%";
                    specification.Where(p =>
                        EF.Functions.ILike(p.Name, pattern, @"\") ||
                        EF.Functions.ILike(p.Manufacturer, pattern, @"\"));
                }
            }
        }

        if (categoryId is not null)
            specification.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            specification.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            specification.Where(p => p.Price.Amount <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(manufacturer))
            specification.Search(p => p.Manufacturer, $"%{manufacturer.Trim()}%");

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
