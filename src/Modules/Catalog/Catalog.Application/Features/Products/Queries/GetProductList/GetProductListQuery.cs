using Catalog.Contracts.Products.GetList;
namespace Catalog.Application.Features.Products.Queries.GetProductList;

public sealed record GetProductListQuery(GetProductsRequest Request) : IQuery<Result<PagedResult<List<ProductListRowDto>>>>;
