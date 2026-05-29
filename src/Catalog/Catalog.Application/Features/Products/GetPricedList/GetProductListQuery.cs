using Catalog.Contracts.Products.GetList;
namespace Catalog.Application.Features.Products.GetPricedList;

public sealed record GetProductListQuery(GetProductsRequest Request) : IQuery<Result<PagedResult<List<ProductListRowDto>>>>;
