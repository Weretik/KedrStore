using Catalog.Contracts.Products.GetAdminDetail;

namespace Catalog.Application.Features.Products.GetAdminDetail;

public sealed record GetAdminProductBySlugQuery(string ProductSlug)
    : IQuery<Result<AdminProductDetailsDto>>;
