using Catalog.Application.Features.Products.Queries.GetAdminProductList;
using Catalog.Application.Features.Products.Shared;
using Catalog.Contracts.Products.GetAdminList;

namespace Catalog.Application.Features.Products.Queries.GetAllAdminProducts;

public sealed record GetAllAdminProductsQuery : IQuery<Result<List<AdminProductListRowDto>>>;
