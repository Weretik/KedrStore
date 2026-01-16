using Catalog.Application.Integrations.OneC.DTOs;

namespace Catalog.Application.Features.Products.GetList;

public sealed record GetProductListQuery(GetProductsRequest Request) : IQuery<Result<PagedResult<List<ProductListRowDto>>>>;
