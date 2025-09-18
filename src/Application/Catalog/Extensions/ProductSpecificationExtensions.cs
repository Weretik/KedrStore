namespace Application.Catalog.Extensions;

public static class ProductFiltersSpecificationExtensions
{
    public static ISpecificationBuilder<Product> ApplyCommonFilters(
        this ISpecificationBuilder<Product> specification,
        ProductsFilter filter)
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
                    specification.Where(p =>
                        EF.Functions.ILike(p.Name, pattern, @"\") ||
                        EF.Functions.ILike(p.Manufacturer, pattern, @"\"));
                }
            }
        }

        if (filter.CategoryId is not null)
            specification.Where(p => p.CategoryId == filter.CategoryId);

        if (filter.MinPrice.HasValue)
            specification.Where(p => p.Price.Amount >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            specification.Where(p => p.Price.Amount <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
            specification.Search(p => p.Manufacturer, $"%{filter.Manufacturer.Trim()}%");

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
