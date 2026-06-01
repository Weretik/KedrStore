using Catalog.Contracts.Products.GetList;
namespace Catalog.Application.Features.Products.GetSalesList;

public sealed record GetProductListQuery(GetProductsRequest Request) : IQuery<Result<PagedResult<List<ProductListRowDto>>>>;
