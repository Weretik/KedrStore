namespace Application.Catalog.Extensions;

public static class ProductFiltersSpecificationExtensions
{
    public static ISpecificationBuilder<Product> ApplyCommonFilters(
        this ISpecificationBuilder<Product> specification,
        ProductsCriteria criteria)
    {

        ArgumentNullException.ThrowIfNull(criteria);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var term = criteria.SearchTerm!.Trim();
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

        if (criteria.CategoryId is not null)
            specification.Where(p => p.CategoryId == criteria.CategoryId);

        if (criteria.MinPrice.HasValue)
            specification.Where(p => p.Price.Amount >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            specification.Where(p => p.Price.Amount <= criteria.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(criteria.Manufacturer))
            specification.Search(p => p.Manufacturer, $"%{criteria.Manufacturer.Trim()}%");

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
