using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Products.GetAdminList;

namespace Catalog.Application.Features.Products.GetAdminList;

public sealed class GetAllAdminProductsQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetAllAdminProductsQuery, Result<List<AdminProductListRowDto>>>
{
    public async ValueTask<Result<List<AdminProductListRowDto>>> Handle(
        GetAllAdminProductsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await (
                from projection in catalogDbContext.ProductListProjections.AsNoTracking()
                join product in catalogDbContext.Products.AsNoTracking() on projection.ProductId equals product.Id
                orderby projection.ProductId
                select new AdminProductListRowDto
                {
                    Id = projection.ProductId.Value,
                    NameUk = projection.NameUk,
                    NameRu = projection.NameRu,
                    ProductSlug = projection.ProductSlug,
                    Photo = projection.Photo,
                    CategoryId = projection.CategoryId.Value,
                    InStock = projection.InStock,
                    IsSale = projection.IsSale,
                    IsNew = projection.IsNew,
                    ExportToSite = projection.ExportToSite,
                    Price = projection.RetailPrice,
                    Stock = product.Stock,
                    QuantityInPack = product.QuantityInPack
                })
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
