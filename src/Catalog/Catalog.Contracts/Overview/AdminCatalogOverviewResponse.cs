namespace Catalog.Contracts.Overview;

/// <summary>Catalog problems for the admin «Огляд»: only counts of what needs fixing.</summary>
/// <param name="ProductsWithoutPhoto">Active products without a linked photo, as the «Без фото» tab lists them.</param>
/// <param name="TopCategoryWithoutPhoto">The category with most of them, or <c>null</c> when there are none.</param>
/// <param name="CategoriesWithoutText">Active categories without a site text or FAQ in Ukrainian or Russian.</param>
public sealed record AdminCatalogOverviewResponse(
    int ProductsWithoutPhoto,
    AdminCatalogOverviewCategory? TopCategoryWithoutPhoto,
    int CategoriesWithoutText);

public sealed record AdminCatalogOverviewCategory(int Id, string Name, int Count);
