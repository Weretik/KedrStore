using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Querying;
using Catalog.Application.Slugs;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.PublicListing;

/// <summary>
/// The products a public list may show for a request, before attribute filters and sorting: on the site, outside
/// hidden roots, in the selected category tree, and matching search, stock, sale, new, and price.
/// </summary>
public sealed class PublicProductQuery(
    IReadCatalogDbContext db,
    ISlugService slugService,
    IOptions<RootCategoryId> roots)
{
    /// <summary>The query, or <c>null</c> when the requested category is unknown or deleted.</summary>
    public async Task<IQueryable<ProductListProjection>?> BuildAsync(GetProductsRequest request, string language,
        CancellationToken cancellationToken)
    {
        var query = db.ProductListProjections.AsNoTracking().Where(projection => projection.ExportToSite);
        var hiddenCategoryIds = PublicCatalogScope.HiddenCategoryIds(db.Categories.AsNoTracking(), roots.Value);
        query = query.Where(projection => !hiddenCategoryIds.Contains(projection.CategoryId));

        var categoryIds = await SelectedCategoryTreeIdsAsync(request, cancellationToken);
        if (categoryIds is { Length: 0 } && !string.IsNullOrWhiteSpace(request.CategorySlug) && request.CategoryId is null)
            return null;
        if (categoryIds is not null)
            query = query.Where(projection => categoryIds.Contains(projection.CategoryId));

        return ApplyFilters(query, request, language);
    }

    /// <summary>
    /// Selected category with descendants; empty when the category is unknown or deleted. A former category slug
    /// still selects its category (R-017).
    /// </summary>
    public async Task<ProductCategoryId[]?> SelectedCategoryTreeIdsAsync(GetProductsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CategoryId is null && string.IsNullOrWhiteSpace(request.CategorySlug))
            return null;

        var selectedId = request.CategoryId
            ?? await slugService.ResolveAsync(CatalogEntityType.Category, request.CategorySlug!, cancellationToken);
        if (selectedId is null)
            return [];

        return await CategoryTreeIds.ForSelectedTreeAsync(db.Categories.AsNoTracking(), selectedId.Value,
            cancellationToken);
    }

    private static IQueryable<ProductListProjection> ApplyFilters(IQueryable<ProductListProjection> query,
        GetProductsRequest request, string language)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var tokens = request.SearchTerm.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(LikePattern.Escape)
                .ToArray();

            foreach (var token in tokens)
            {
                var pattern = $"%{token}%";
                query = language == "ru"
                    ? query.Where(projection => EF.Functions.ILike(projection.SearchTextRu, pattern, @"\"))
                    : query.Where(projection => EF.Functions.ILike(projection.SearchTextUk, pattern, @"\"));
            }
        }

        var inStockOnly = request.Availability is { } availability
            ? availability == ProductAvailability.InStock
            : request.InStock == true;
        if (inStockOnly)
            query = query.Where(projection => projection.InStock);

        if (request.IsSale.HasValue)
            query = query.Where(projection => projection.IsSale == request.IsSale.Value);

        if (request.IsNew.HasValue)
            query = query.Where(projection => projection.IsNew == request.IsNew.Value);

        if (request.PriceFrom is not null)
            query = query.Where(projection => projection.RetailPrice != null && projection.RetailPrice >= request.PriceFrom.Value);

        if (request.PriceTo is not null)
            query = query.Where(projection => projection.RetailPrice != null && projection.RetailPrice <= request.PriceTo.Value);

        return query;
    }
}
