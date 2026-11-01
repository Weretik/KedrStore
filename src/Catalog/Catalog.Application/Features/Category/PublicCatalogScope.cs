using Catalog.Application.Integrations.OneC.Options;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category;

/// <summary>
/// Import roots the public catalog never shows. Cosmos is imported for internal use only, so its categories and
/// products stay out of the public tree, category pages, and product lists (owner decision 2026-11-01).
/// </summary>
internal static class PublicCatalogScope
{
    public static bool IsHidden(ProductCategory category, RootCategoryId roots)
        => string.Equals(category.ProductTypeIdOneC, roots.CosmosRootCategoryId, StringComparison.Ordinal);

    /// <summary>IDs of hidden categories, as a query to compose into product filters.</summary>
    public static IQueryable<ProductCategoryId> HiddenCategoryIds(
        IQueryable<ProductCategory> categories, RootCategoryId roots)
    {
        var hiddenRoot = roots.CosmosRootCategoryId;
        return categories
            .Where(category => category.ProductTypeIdOneC == hiddenRoot)
            .Select(category => category.Id);
    }
}
