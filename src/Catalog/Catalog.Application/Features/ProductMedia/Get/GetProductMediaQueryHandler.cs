using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductMedia.Common;
using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Get;

public sealed class GetProductMediaQueryHandler(ICatalogReadRepository<Product> products, IProductMediaLibrary library)
    : IQueryHandler<GetProductMediaQuery, Result<ProductMediaDto>>
{
    public async ValueTask<Result<ProductMediaDto>> Handle(GetProductMediaQuery query,
        CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(
            new ProductForMediaSpec(ProductId.From(query.ProductId)), cancellationToken);
        if (product is null)
            return Result.NotFound();

        var files = await library.GetByIdsAsync(
            product.MediaLinks.Select(link => link.MediaFileId).ToArray(), cancellationToken);
        return Result.Success(ProductMediaMapper.ToDto(product, files));
    }
}
