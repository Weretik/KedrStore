using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.OneCSync.Sync;

public sealed class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
