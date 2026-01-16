using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetList;

public static class ProductListFiltersExtension
{
    public static IQueryable<Product> ApplyProductListFilters(this IQueryable<Product> productsQuery, GetProductsRequest request)
    {
        ArgumentNullException.ThrowIfNull(productsQuery);
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm!.Trim();
            var tokens = term.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
            {
                productsQuery = productsQuery.Where(x => x.Id == ProductId.From(id));
            }
            else
            {
                productsQuery = productsQuery.Where(x =>
                    tokens.All(raw =>
                        EF.Functions.ILike(
                            x.Name,
                            $"%{EscapeLike(raw)}%",
                            @"\"
                        )
                    )
                );
            }
        }

        if (request.CategoryId.HasValue)
        {
            var filterCategoryId = ProductCategoryId.From(request.CategoryId.Value);
            productsQuery = productsQuery.Where(p => p.CategoryId == filterCategoryId);
        }

        if (request.InStock == true)
            productsQuery = productsQuery.Where(p => p.Stock > 0);

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
}
