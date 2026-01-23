using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetList;

public static class ProductListFiltersExtension
{
    public static IQueryable<Product> ApplyProductListFilters(this IQueryable<Product> productsQuery, GetProductsRequest request, string hardwareRootCategoryId)
    {
        ArgumentNullException.ThrowIfNull(productsQuery);
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm!.Trim();

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
            {
                productsQuery = productsQuery.Where(x => x.Id == ProductId.From(id));
            }
            else
            {
                var escapedTokens = term
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(EscapeLike)
                    .ToArray();

                foreach (var token  in escapedTokens)
                {
                    productsQuery = productsQuery.Where(x =>
                        EF.Functions.ILike(x.Name, $"%{token}%", @"\"));
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var categoryId = TryGetTrailingIntId(request.CategorySlug);

            if (categoryId is not null)
            {
                var filterCategoryId = ProductCategoryId.From(categoryId.Value);
                productsQuery = productsQuery.Where(p => p.CategoryId == filterCategoryId);
            }
        }

        if (request.InStock == true)
        {
            productsQuery = productsQuery.Where(p =>
                (p.ProductTypeIdOneC == hardwareRootCategoryId && p.Stock > 2) ||
                (p.ProductTypeIdOneC != hardwareRootCategoryId && p.Stock > 0)
            );
        }


        if (request.IsSale.HasValue)
            productsQuery = productsQuery.Where(p => p.IsSale == request.IsSale.Value);

        if (request.IsNew.HasValue)
            productsQuery = productsQuery.Where(p => p.IsNew == request.IsNew.Value);

        return productsQuery;
    }
    private static string EscapeLike(string text)
    {
        return text
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }

    private static int? TryGetTrailingIntId(string slug)
    {
        var lastDash = slug.LastIndexOf('-');
        if (lastDash < 0 || lastDash == slug.Length - 1)
            return null;

        var tail = slug[(lastDash + 1)..];

        if (int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
            return id;

        return null;
    }
}
