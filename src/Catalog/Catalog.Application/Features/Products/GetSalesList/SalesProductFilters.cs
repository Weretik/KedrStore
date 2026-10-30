using Catalog.Application.Querying;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetSalesList;

/// <summary>Product filters of the sales catalog that apply before prices are joined.</summary>
internal static class SalesProductFilters
{
    /// <summary>
    /// Search (name tokens or an exact ID), category with two levels of descendants, stock, sale and new flags.
    /// The sales catalog filters by <see cref="GetProductsRequest.CategoryId"/> only; a category slug is ignored.
    /// </summary>
    public static IQueryable<Product> Apply(
        IQueryable<Product> productsQuery,
        IQueryable<ProductCategory> categoriesQuery,
        GetProductsRequest request,
        string hardwareRootCategoryId)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            productsQuery = ApplySearch(productsQuery, request.SearchTerm.Trim());

        if (request.CategoryId is not null)
        {
            productsQuery = ApplyCategory(
                productsQuery, categoriesQuery, ProductCategoryId.From(request.CategoryId.Value));
        }

        if (request.InStock == true)
        {
            productsQuery = productsQuery.Where(p =>
                (p.ProductTypeIdOneC == hardwareRootCategoryId && p.Stock > 2) ||
                (p.ProductTypeIdOneC != hardwareRootCategoryId && p.Stock > 0));
        }

        if (request.IsSale.HasValue)
            productsQuery = productsQuery.Where(p => p.IsSale == request.IsSale.Value);

        if (request.IsNew.HasValue)
            productsQuery = productsQuery.Where(p => p.IsNew == request.IsNew.Value);

        return productsQuery;
    }

    private static IQueryable<Product> ApplySearch(IQueryable<Product> productsQuery, string term)
    {
        var tokens = term
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(LikePattern.Escape)
            .ToArray();

        var nameQuery = productsQuery;
        foreach (var token in tokens)
            nameQuery = nameQuery.Where(x => EF.Functions.ILike(x.Name, $"%{token}%", LikePattern.EscapeCharacter));

        if (!int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || id <= 0)
            return nameQuery;

        var productId = ProductId.From(id);
        return productsQuery.Where(x => x.Id == productId || nameQuery.Any(p => p.Id == x.Id));
    }

    private static IQueryable<Product> ApplyCategory(
        IQueryable<Product> productsQuery,
        IQueryable<ProductCategory> categoriesQuery,
        ProductCategoryId selectedCategoryId)
    {
        var childCategoryIds = categoriesQuery
            .Where(category => category.ParentId == selectedCategoryId)
            .Select(category => category.Id);

        var categoryIds = categoriesQuery
            .Where(category =>
                category.Id == selectedCategoryId ||
                category.ParentId == selectedCategoryId ||
                (category.ParentId.HasValue && childCategoryIds.Contains(category.ParentId.Value)))
            .Select(category => category.Id);

        return productsQuery.Where(product => categoryIds.Contains(product.CategoryId));
    }
}
