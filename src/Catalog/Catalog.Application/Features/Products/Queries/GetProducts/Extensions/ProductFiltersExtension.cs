using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

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
                specification.Where(p => p.Id == ProductId.From(id));
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

        if (filter.CategoryId.HasValue)
        {
            var filterCategoryId = ProductCategoryId.From(filter.CategoryId.Value);
            specification.Where(p => p.CategoryId == filterCategoryId);
        }


        //Price filter
        var hasMin = pricingOptions.MinPrice.HasValue;
        var hasMax = pricingOptions.MaxPrice.HasValue;
        var min = pricingOptions.MinPrice.GetValueOrDefault();
        var max = pricingOptions.MaxPrice.GetValueOrDefault();

        specification.Where(p => p.Stock > 0);

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
