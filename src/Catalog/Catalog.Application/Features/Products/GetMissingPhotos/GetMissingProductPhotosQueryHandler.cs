using Catalog.Application.Querying;
using Catalog.Application.Features.Category;
using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetMissingPhotos;

public sealed class GetMissingProductPhotosQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetMissingProductPhotosQuery, Result<PagedResult<List<MissingProductPhotoListRowDto>>>>
{
    public async ValueTask<Result<PagedResult<List<MissingProductPhotoListRowDto>>>> Handle(
        GetMissingProductPhotosQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = query.Request.Page;
        var pageSize = query.Request.PageSize;
        var categoriesQuery = catalogDbContext.Categories.AsNoTracking();
        var categoryIds = query.Request.CategoryId is { } categoryId
            ? await CategoryTreeIds.ForSelectedTreeAsync(categoriesQuery, categoryId, cancellationToken)
            : null;
        var projections = ApplyFilters(
            catalogDbContext.ProductListProjections.AsNoTracking(),
            categoryIds,
            query.Request);
        var unavailable =
            from projection in projections
            join product in catalogDbContext.Products.AsNoTracking()
                on projection.ProductId equals product.Id
            join check in catalogDbContext.ProductPhotoChecks.AsNoTracking()
                on product.Id equals check.Id
            where check.Status == ProductPhotoStatus.Missing ||
                  check.Status == ProductPhotoStatus.InvalidContentType ||
                  check.Status == ProductPhotoStatus.CheckFailed
            select new { projection, product, check };

        var totalRecords = await unavailable.LongCountAsync(cancellationToken);
        var sorted = query.Request.Sort switch
        {
            ProductSort.IdDesc => unavailable.OrderByDescending(row => row.projection.ProductId),
            ProductSort.NameAsc => unavailable.OrderBy(row => row.projection.NameUk)
                .ThenBy(row => row.projection.NameRu).ThenBy(row => row.projection.ProductId),
            ProductSort.NameDesc => unavailable.OrderByDescending(row => row.projection.NameUk)
                .ThenByDescending(row => row.projection.NameRu).ThenBy(row => row.projection.ProductId),
            ProductSort.PriceAsc => unavailable.OrderBy(row => row.projection.RetailPrice.HasValue ? 0 : 1)
                .ThenBy(row => row.projection.RetailPrice).ThenBy(row => row.projection.ProductId),
            ProductSort.PriceDesc => unavailable.OrderBy(row => row.projection.RetailPrice.HasValue ? 0 : 1)
                .ThenByDescending(row => row.projection.RetailPrice).ThenBy(row => row.projection.ProductId),
            ProductSort.CategoryAsc => unavailable.OrderBy(row => categoriesQuery
                    .Where(category => category.Id == row.projection.CategoryId)
                    .Select(category => category.ShortNameUk).FirstOrDefault())
                .ThenBy(row => row.projection.ProductId),
            ProductSort.CategoryDesc => unavailable.OrderByDescending(row => categoriesQuery
                    .Where(category => category.Id == row.projection.CategoryId)
                    .Select(category => category.ShortNameUk).FirstOrDefault())
                .ThenBy(row => row.projection.ProductId),
            _ => unavailable.OrderBy(row => row.projection.ProductId)
        };
        var items = await sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new MissingProductPhotoListRowDto
            {
                Id = row.projection.ProductId.Value,
                NameUk = row.projection.NameUk,
                NameRu = row.projection.NameRu,
                ProductSlug = row.projection.ProductSlug,
                Photo = row.check.PhotoUrl,
                CategoryId = row.projection.CategoryId.Value,
                CategoryName = categoriesQuery.Where(category => category.Id == row.projection.CategoryId)
                    .Select(category => category.ShortNameUk).FirstOrDefault() ?? string.Empty,
                InStock = row.projection.InStock,
                IsSale = row.projection.IsSale,
                IsNew = row.projection.IsNew,
                ExportToSite = row.projection.ExportToSite,
                Price = row.projection.RetailPrice,
                Stock = row.product.Stock,
                QuantityInPack = row.product.QuantityInPack,
                PhotoAvailable = false,
                PhotoStatus = row.check.Status == ProductPhotoStatus.Missing
                    ? ProductPhotoAvailabilityStatus.Missing
                    : row.check.Status == ProductPhotoStatus.InvalidContentType
                        ? ProductPhotoAvailabilityStatus.InvalidContentType
                        : ProductPhotoAvailabilityStatus.CheckFailed,
                PhotoCheckedAtUtc = row.check.CheckedAtUtc!.Value,
                PhotoHttpStatusCode = row.check.HttpStatusCode
            })
            .ToListAsync(cancellationToken);

        var pagedInfo = new PagedInfo(
            page,
            pageSize,
            (long)Math.Ceiling(totalRecords / (double)pageSize),
            totalRecords);

        return Result.Success(new PagedResult<List<MissingProductPhotoListRowDto>>(pagedInfo, items));
    }

    private static IQueryable<ProductListProjection> ApplyFilters(
        IQueryable<ProductListProjection> projections,
        ProductCategoryId[]? categoryIds,
        GetMissingProductPhotosRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var tokens = request.SearchTerm.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(LikePattern.Escape);

            foreach (var token in tokens)
            {
                var pattern = $"%{token}%";
                projections = projections.Where(projection =>
                    EF.Functions.ILike(projection.SearchTextUk, pattern, @"\") ||
                    EF.Functions.ILike(projection.SearchTextRu, pattern, @"\"));
            }
        }

        if (categoryIds is not null)
            projections = projections.Where(projection => categoryIds.Contains(projection.CategoryId));

        if (request.InStock.HasValue)
            projections = projections.Where(projection => projection.InStock == request.InStock.Value);

        if (request.IsSale.HasValue)
            projections = projections.Where(projection => projection.IsSale == request.IsSale.Value);

        if (request.IsNew.HasValue)
            projections = projections.Where(projection => projection.IsNew == request.IsNew.Value);

        if (request.ExportToSite.HasValue)
            projections = projections.Where(projection => projection.ExportToSite == request.ExportToSite.Value);

        return projections;
    }
}
