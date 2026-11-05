using Catalog.Application.Integrations.OneC.Options;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category;

/// <summary>
/// Categories the public catalog shows: those an admin marked «show on site» (spec catalog/007 R-18). The Cosmos import
/// root stays hidden in any case: it is imported for internal use only (owner decision 2026-11-01).
/// </summary>
internal static class PublicCatalogScope
{
    public static bool IsHidden(ProductCategory category, RootCategoryId roots)
        => !category.IsShownOnSite ||
           string.Equals(category.ProductTypeIdOneC, roots.CosmosRootCategoryId, StringComparison.Ordinal);

    /// <summary>IDs of hidden categories, as a query to compose into product filters.</summary>
    public static IQueryable<ProductCategoryId> HiddenCategoryIds(
        IQueryable<ProductCategory> categories, RootCategoryId roots)
    {
        var hiddenRoot = roots.CosmosRootCategoryId;
        return categories
            .Where(category => !category.IsShownOnSite || category.ProductTypeIdOneC == hiddenRoot)
            .Select(category => category.Id);
    }
}
