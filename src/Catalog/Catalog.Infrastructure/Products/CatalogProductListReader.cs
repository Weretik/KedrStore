namespace Catalog.Infrastructure.Products;

internal sealed class CatalogProductListReader(ISender sender) : ICatalogProductListReader
{
    public async Task<Result<PagedResult<List<ProductListRowDto>>>> GetListAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetProductListQuery(request), cancellationToken);
    }
}
