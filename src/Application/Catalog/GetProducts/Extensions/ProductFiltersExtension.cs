using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;

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

        if (filter.ProductTypeId.HasValue)
        {
            var filterProductTypeId = ProductType.FromValue(filter.ProductTypeId.Value);
            specification.Where(p => p.ProductType == filterProductTypeId);
        }

        //Price filter
        var priceTypeVo = PriceType.FromName(pricingOptions.PriceType, false);
        var hasMin = pricingOptions.MinPrice.HasValue;
        var hasMax = pricingOptions.MaxPrice.HasValue;
        var min = pricingOptions.MinPrice.GetValueOrDefault();
        var max = pricingOptions.MaxPrice.GetValueOrDefault();

        specification.Where(p =>
            p.Stock > 0
            &&
            p.Prices.Any(pp =>
                pp.PriceType == priceTypeVo
                && pp.Amount > 0
                && (!hasMin || pp.Amount >= min)
                && (!hasMax || pp.Amount <= max)));

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
