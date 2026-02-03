using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Options;

namespace Catalog.Application.Features.Products.GetList;

public class GetProductListQueryHandler(IReadCatalogDbContext catalogDbContext, IOptionsSnapshot<RootCategoryId> options)
    : IQueryHandler<GetProductListQuery, Result<PagedResult<List<ProductListRowDto>>>>
{
    public async ValueTask<Result<PagedResult<List<ProductListRowDto>>>> Handle(
        GetProductListQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var hardwareRootCategoryId = options.Value.HardwareRootCategoryId;
        var request = query.Request;

        var productQuery = catalogDbContext.Products.AsNoTracking();
        var priceQuery = catalogDbContext.ProductPrices.AsNoTracking();

        productQuery = productQuery.ApplyProductListFilters(request, hardwareRootCategoryId);

        var productListQuery = productQuery
            .JoinPricesForList(priceQuery, request, hardwareRootCategoryId)
            .ApplySorting(request.Sort);

        const int maxPageSize = 100;
        var totalRecords  = await productListQuery.CountAsync(cancellationToken);
        var pageNumber  = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var totalPages = (long)Math.Ceiling(totalRecords / (double)pageSize);
        var skip = (pageNumber - 1) * pageSize;

        productListQuery = productListQuery.Skip(skip).Take(pageSize);

        var items = await productListQuery.ToListAsync(cancellationToken);

        var pagedInfo = new PagedInfo(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalPages: totalPages,
            totalRecords: totalRecords);

        var result = new PagedResult<List<ProductListRowDto>>(pagedInfo, items);

        return Result.Success(result);

    }
}
