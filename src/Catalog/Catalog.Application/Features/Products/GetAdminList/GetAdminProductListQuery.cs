using Catalog.Contracts.Products.GetAdminList;

namespace Catalog.Application.Features.Products.GetAdminList;

public sealed record GetAdminProductListQuery(GetAdminProductsRequest Request)
    : IQuery<Result<PagedResult<List<AdminProductListRowDto>>>>;
