using Catalog.Contracts.Products.GetAdminDetail;

namespace Catalog.Application.Features.Products.Queries.GetAdminProductBySlug;

public sealed record GetAdminProductBySlugQuery(string ProductSlug)
    : IQuery<Result<AdminProductDetailsDto>>;
