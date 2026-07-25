using Catalog.Contracts.Products.GetAdminList;

namespace Catalog.Application.Features.Products.GetAdminList;

public sealed record GetAllAdminProductsQuery : IQuery<Result<List<AdminProductListRowDto>>>;
