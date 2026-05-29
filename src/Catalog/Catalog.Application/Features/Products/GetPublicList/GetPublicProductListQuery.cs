using Catalog.Contracts.Products.GetList;

namespace Catalog.Application.Features.Products.GetPublicList;

public sealed record GetPublicProductListQuery(GetProductsRequest Request)
    : IQuery<Result<PagedResult<List<ProductListRowDto>>>>;
