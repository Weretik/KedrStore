using Catalog.Application.Features.Products.Queries.GetAllAdminProducts;
using Catalog.Application.Features.Products.Shared;
using Catalog.Contracts.Products.GetAdminList;

namespace Catalog.Application.Features.Products.Queries.GetAdminProductList;

public sealed record GetAdminProductListQuery(GetAdminProductsRequest Request)
    : IQuery<Result<PagedResult<List<AdminProductListRowDto>>>>;
