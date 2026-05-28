namespace Sales.Application.Features.Catalog.GetList;

public sealed class GetCatalogListQueryHandler(ICatalogProductReader catalogProductReader)
    : IQueryHandler<GetCatalogListQuery, Result<PagedResult<List<CatalogListItemDto>>>>
{
    public async ValueTask<Result<PagedResult<List<CatalogListItemDto>>>> Handle(
        GetCatalogListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await catalogProductReader.GetListAsync(query.Request, cancellationToken);
    }
}
