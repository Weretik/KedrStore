using Catalog.Application.Features.Products.GetList.DTOs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetList.Extensions;

public static class ProductListFiltersExtension
{
    public static IQueryable<Product> ApplyProductListFilters(
        this IQueryable<Product> productsQuery,
        IQueryable<ProductCategory> categoriesQuery,
        GetProductsRequest request,
        string hardwareRootCategoryId)
    {
        ArgumentNullException.ThrowIfNull(productsQuery);
        ArgumentNullException.ThrowIfNull(categoriesQuery);
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm!.Trim();

            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike)
                .ToArray();

            var hasId = int.TryParse(term,NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0;

            var nameQuery = productsQuery;
            foreach (var token in tokens)
            {
                nameQuery = nameQuery.Where(x =>
                    EF.Functions.ILike(x.Name, $"%{token}%", @"\")
                    );
            }

            if (!hasId)
            {
                productsQuery = nameQuery;

            }
            else
            {
                var productId = ProductId.From(id);

                productsQuery = productsQuery.Where(x =>
                    x.Id == productId ||
                    nameQuery.Any(p => p.Id == x.Id)
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var categoryId = TryGetTrailingIntId(request.CategorySlug);

            if (categoryId is not null)
            {
                var selectedCategoryId = ProductCategoryId.From(categoryId.Value);

                var childCategoryIds = categoriesQuery
                    .Where(category => category.ParentId == selectedCategoryId)
                    .Select(category => category.Id);

                var categoryIds = categoriesQuery
                    .Where(category =>
                        category.Id == selectedCategoryId ||
                        category.ParentId == selectedCategoryId ||
                        (category.ParentId.HasValue && childCategoryIds.Contains(category.ParentId.Value)))
                    .Select(category => category.Id);

                productsQuery = productsQuery.Where(product => categoryIds.Contains(product.CategoryId));
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
