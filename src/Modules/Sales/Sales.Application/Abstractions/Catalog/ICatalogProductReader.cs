namespace Sales.Application.Abstractions.Catalog;

public interface ICatalogProductReader
{
    Task<Result<PagedResult<List<CatalogListItemDto>>>> GetListAsync(
        CatalogRequest request,
        CancellationToken cancellationToken);
}
