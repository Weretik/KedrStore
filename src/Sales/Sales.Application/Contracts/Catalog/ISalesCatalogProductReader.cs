namespace Sales.Application.Contracts.Catalog;

public interface ISalesCatalogProductReader
{
    Task<Result<PagedResult<List<SalesCatalogListItemDto>>>> GetListAsync(
        GetSalesCatalogRequest request,
        CancellationToken cancellationToken);
}
