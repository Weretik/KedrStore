namespace Sales.Application.Features.Catalog.GetList;

public sealed class GetSalesCatalogListQueryHandler(ISalesCatalogProductReader catalogProductReader)
    : IQueryHandler<GetSalesCatalogListQuery, Result<PagedResult<List<SalesCatalogListItemDto>>>>
{
    public async ValueTask<Result<PagedResult<List<SalesCatalogListItemDto>>>> Handle(
        GetSalesCatalogListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await catalogProductReader.GetListAsync(query.Request, cancellationToken);
    }
}
