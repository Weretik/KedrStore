using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;

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
        var projections = ApplyFilters(
            catalogDbContext.ProductListProjections.AsNoTracking(),
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
            select new MissingProductPhotoListRowDto
            {
                Id = projection.ProductId.Value,
                NameUk = projection.NameUk,
                NameRu = projection.NameRu,
                ProductSlug = projection.ProductSlug,
                Photo = check.PhotoUrl,
                CategoryId = projection.CategoryId.Value,
                InStock = projection.InStock,
                IsSale = projection.IsSale,
                IsNew = projection.IsNew,
                ExportToSite = projection.ExportToSite,
                Price = projection.RetailPrice,
                Stock = product.Stock,
                QuantityInPack = product.QuantityInPack,
                PhotoAvailable = false,
                PhotoStatus = check.Status == ProductPhotoStatus.Missing
                    ? ProductPhotoAvailabilityStatus.Missing
                    : check.Status == ProductPhotoStatus.InvalidContentType
                        ? ProductPhotoAvailabilityStatus.InvalidContentType
                        : ProductPhotoAvailabilityStatus.CheckFailed,
                PhotoCheckedAtUtc = check.CheckedAtUtc!.Value,
                PhotoHttpStatusCode = check.HttpStatusCode
            };

        var totalRecords = await unavailable.LongCountAsync(cancellationToken);
        var items = await ApplySorting(unavailable, query.Request.Sort)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
        GetMissingProductPhotosRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var tokens = request.SearchTerm.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike);

            foreach (var token in tokens)
            {
                var pattern = $"%{token}%";
                projections = projections.Where(projection =>
                    EF.Functions.ILike(projection.SearchTextUk, pattern, @"\") ||
                    EF.Functions.ILike(projection.SearchTextRu, pattern, @"\"));
            }
        }

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

    private static IOrderedQueryable<MissingProductPhotoListRowDto> ApplySorting(
        IQueryable<MissingProductPhotoListRowDto> products,
        ProductSort sort) => sort switch
        {
            ProductSort.IdDesc => products.OrderByDescending(product => product.Id),
            ProductSort.NameAsc => products.OrderBy(product => product.NameUk).ThenBy(product => product.NameRu).ThenBy(product => product.Id),
            ProductSort.NameDesc => products.OrderByDescending(product => product.NameUk).ThenByDescending(product => product.NameRu).ThenBy(product => product.Id),
            ProductSort.PriceAsc => products.OrderBy(product => product.Price.HasValue ? 0 : 1).ThenBy(product => product.Price).ThenBy(product => product.Id),
            ProductSort.PriceDesc => products.OrderBy(product => product.Price.HasValue ? 0 : 1).ThenByDescending(product => product.Price).ThenBy(product => product.Id),
            _ => products.OrderBy(product => product.Id)
        };

    private static string EscapeLike(string text) => text
        .Replace(@"\", @"\\", StringComparison.Ordinal)
        .Replace("%", @"\%", StringComparison.Ordinal)
        .Replace("_", @"\_", StringComparison.Ordinal);
}
