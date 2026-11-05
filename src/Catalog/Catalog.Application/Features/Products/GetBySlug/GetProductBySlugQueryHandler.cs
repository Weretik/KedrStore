using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.GetBySlug.DTOs;
using Catalog.Application.Features.Products.GetBySlug.Extensions;
using Catalog.Application.Slugs;
using Catalog.Contracts.Category;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetBySlug;

/// <summary>Public product page: current or former slug resolves; a deleted product points to its category (R-018).</summary>
public class GetProductBySlugQueryHandler(IReadCatalogDbContext catalogDbContext, ISlugService slugService)
    : IQueryHandler<GetProductBySlugQuery, Result<CatalogLookup<ProductBySlugDto>>>
{
    public async ValueTask<Result<CatalogLookup<ProductBySlugDto>>> Handle(GetProductBySlugQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var request = query.Request;
        var slug = request.Slug.Trim().ToLowerInvariant();
        var productId = await slugService.ResolveAsync(CatalogEntityType.Product, slug, cancellationToken)
                        ?? await FindDeletedBySlugAsync(slug, cancellationToken);
        if (productId is null)
            return Result.NotFound();

        var result = await catalogDbContext.Products
            .AsNoTracking()
            .JoinPricesAndCategoryForProductBySlug(
                catalogDbContext.ProductPrices.AsNoTracking(),
                catalogDbContext.Categories.AsNoTracking(),
                catalogDbContext.ProductTranslations.AsNoTracking(),
                productId.Value,
                request)
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return Result.Success(CatalogLookup<ProductBySlugDto>.Gone(await RedirectForAsync(productId.Value, cancellationToken)));

        var breadcrumbs = await catalogDbContext.Categories
            .AsNoTracking()
            .BuildCategoryBreadcrumbsAsync(result.CategorySlug, cancellationToken);

        return Result.Success(CatalogLookup<ProductBySlugDto>.Found(result with { Breadcrumbs = breadcrumbs }));
    }

    private Task<int?> FindDeletedBySlugAsync(string slug, CancellationToken cancellationToken)
        => catalogDbContext.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.IsDeleted && product.ProductSlug == slug)
            .Select(product => (int?)product.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Slug of the active category of a deleted product, or <c>null</c>.</summary>
    private async Task<CatalogRedirect?> RedirectForAsync(int productId, CancellationToken cancellationToken)
    {
        var id = ProductId.From(productId);
        var categorySlug = await (
                from product in catalogDbContext.Products.IgnoreQueryFilters().AsNoTracking()
                join category in catalogDbContext.Categories.AsNoTracking() on product.CategoryId equals category.Id
                where product.Id == id
                select category.Slug)
            .FirstOrDefaultAsync(cancellationToken);

        return categorySlug is null ? null : new CatalogRedirect(categorySlug);
    }
}
