using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.OneCSync.Sync;

public sealed class ProductPriceByProductAndTypeSpec : Specification<ProductPrice>
{
    public ProductPriceByProductAndTypeSpec(ProductId productId, PriceTypeId priceTypeId)
    {
        Query.Where(p => p.ProductId == productId && p.PriceTypeId == priceTypeId);
    }
}
