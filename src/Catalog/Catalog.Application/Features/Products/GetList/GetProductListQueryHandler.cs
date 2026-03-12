using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.GetList.Extensions;
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
        var categoryQuery = catalogDbContext.Categories.AsNoTracking();
        var priceQuery = catalogDbContext.ProductPrices.AsNoTracking();

        productQuery = productQuery.ApplyProductListFilters(categoryQuery, request, hardwareRootCategoryId);
        var isIdSort = request.Sort is ProductSort.IdAsc or ProductSort.IdDesc;

        if (isIdSort)
        {
            productQuery = request.Sort == ProductSort.IdDesc
                ? productQuery.OrderByDescending(p => p.Id)
                : productQuery.OrderBy(p => p.Id);
        }

        var productListWithPriceQuery = productQuery
            .JoinPricesForList(priceQuery, request, hardwareRootCategoryId);

        var productSortListQuery = isIdSort
            ? productListWithPriceQuery
            : productListWithPriceQuery.ApplySorting(request.Sort);

        const int maxPageSize = 100;
        var totalRecords  = await productListWithPriceQuery.CountAsync(cancellationToken);
        var pageNumber  = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var totalPages = (long)Math.Ceiling(totalRecords / (double)pageSize);
        var skip = (pageNumber - 1) * pageSize;

        var productListQuery = productSortListQuery.Skip(skip).Take(pageSize);

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
