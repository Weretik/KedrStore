using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetMissingPhotos;
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
        var unavailable =
            from projection in catalogDbContext.ProductListProjections.AsNoTracking()
            join product in catalogDbContext.Products.AsNoTracking()
                on projection.ProductId equals product.Id
            join check in catalogDbContext.ProductPhotoChecks.AsNoTracking()
                on product.Id equals check.Id
            where check.Status == ProductPhotoStatus.Missing ||
                  check.Status == ProductPhotoStatus.InvalidContentType ||
                  check.Status == ProductPhotoStatus.CheckFailed
            orderby projection.ProductId
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
        var items = await unavailable
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
}
