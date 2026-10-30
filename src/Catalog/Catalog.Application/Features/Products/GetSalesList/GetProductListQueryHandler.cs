using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Contracts.Pricing;
using Catalog.Contracts.Products.GetList;

namespace Catalog.Application.Features.Products.GetSalesList;

/// <summary>
/// Sales catalog page with the customer's prices. Filtering is in <see cref="SalesProductFilters"/>, price joins in
/// <see cref="SalesProductPricing"/>; this handler pages and sorts.
/// </summary>
public class GetProductListQueryHandler(
    IReadCatalogDbContext catalogDbContext,
    IOptionsSnapshot<RootCategoryId> options,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions)
    : IQueryHandler<GetProductListQuery, Result<PagedResult<List<ProductListRowDto>>>>
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public async ValueTask<Result<PagedResult<List<ProductListRowDto>>>> Handle(
        GetProductListQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var request = query.Request;
        var hardwareRootCategoryId = options.Value.HardwareRootCategoryId;
        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? DefaultPageSize : Math.Min(request.PageSize, MaxPageSize);
        var skip = (pageNumber - 1) * pageSize;

        var products = SalesProductFilters.Apply(
            catalogDbContext.Products.AsNoTracking(),
            catalogDbContext.Categories.AsNoTracking(),
            request,
            hardwareRootCategoryId);

        IQueryable<SalesProductRow> JoinPrices(IQueryable<Product> source) => SalesProductPricing.JoinPrices(
            source,
            catalogDbContext.ProductPrices.AsNoTracking(),
            catalogDbContext.ProductTranslations.AsNoTracking(),
            request,
            pricingOptions.Value.RetailPriceTypeId,
            hardwareRootCategoryId);

        var isIdSort = request.Sort is ProductSort.IdAsc or ProductSort.IdDesc;
        var hasPriceFilter = request.PriceFrom is not null || request.PriceTo is not null;
        long totalRecords;
        IQueryable<SalesProductRow> pageRows;

        if (isIdSort && !hasPriceFilter)
        {
            // ID order needs no prices: count and page the products first, then join prices for one page only.
            var orderedProducts = request.Sort == ProductSort.IdDesc
                ? products.OrderByDescending(p => p.Id)
                : products.OrderBy(p => p.Id);

            totalRecords = await products.LongCountAsync(cancellationToken);
            var joined = JoinPrices(orderedProducts.Skip(skip).Take(pageSize));
            pageRows = request.Sort == ProductSort.IdDesc
                ? joined.OrderByDescending(x => x.Id)
                : joined.OrderBy(x => x.Id);
        }
        else
        {
            var rows = JoinPrices(products);
            totalRecords = await rows.LongCountAsync(cancellationToken);
            pageRows = ApplySorting(rows, request.Sort).Skip(skip).Take(pageSize);
        }

        var items = await pageRows.Select(SalesProductRow.ToDto).ToListAsync(cancellationToken);
        var pagedInfo = new PagedInfo(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalPages: (long)Math.Ceiling(totalRecords / (double)pageSize),
            totalRecords: totalRecords);

        return Result.Success(new PagedResult<List<ProductListRowDto>>(pagedInfo, items));
    }

    private static IQueryable<SalesProductRow> ApplySorting(IQueryable<SalesProductRow> query, ProductSort sort)
        => sort switch
        {
            ProductSort.IdAsc => query.OrderBy(x => x.Id),
            ProductSort.IdDesc => query.OrderByDescending(x => x.Id),
            ProductSort.PriceAsc => query.OrderBy(x => x.Price.HasValue ? 0 : 1).ThenBy(x => x.Price),
            ProductSort.PriceDesc => query.OrderBy(x => x.Price.HasValue ? 0 : 1).ThenByDescending(x => x.Price),
            ProductSort.NameDesc => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };
}
