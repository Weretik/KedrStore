using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetAdminDetail;

namespace Catalog.Application.Features.Products.GetAdminDetail;

public sealed class GetAdminProductBySlugQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetAdminProductBySlugQuery, Result<AdminProductDetailsDto>>
{
    public async ValueTask<Result<AdminProductDetailsDto>> Handle(
        GetAdminProductBySlugQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var details = await (
            from projection in catalogDbContext.ProductListProjections.AsNoTracking()
            join product in catalogDbContext.Products.AsNoTracking()
                on projection.ProductId equals product.Id
            join category in catalogDbContext.Categories.AsNoTracking()
                on projection.CategoryId equals category.Id
            where projection.ProductSlug == query.ProductSlug
            select new AdminProductDetailsDto
            {
                Id = projection.ProductId.Value,
                NameUk = projection.NameUk,
                NameRu = projection.NameRu,
                ProductSlug = projection.ProductSlug,
                Photo = projection.Photo,
                Scheme = product.Sсheme,
                CategoryId = projection.CategoryId.Value,
                CategoryName = category.Name,
                InStock = projection.InStock,
                IsSale = projection.IsSale,
                IsNew = projection.IsNew,
                ExportToSite = projection.ExportToSite,
                Price = projection.RetailPrice,
                Stock = product.Stock,
                QuantityInPack = product.QuantityInPack
            }).FirstOrDefaultAsync(cancellationToken);

        return details is null ? Result.NotFound() : Result.Success(details);
    }
}
