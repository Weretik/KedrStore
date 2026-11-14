using Ardalis.Result;

namespace Catalog.Contracts.Products.GetList;

public interface ICatalogProductListReader
{
    Task<Result<PagedResult<List<ProductListRowDto>>>> GetListAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken);
}
