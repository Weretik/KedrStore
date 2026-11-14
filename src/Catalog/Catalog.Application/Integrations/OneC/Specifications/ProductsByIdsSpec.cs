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

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class ProductsByIdsSpec : Specification<Product>
{
    public ProductsByIdsSpec(IEnumerable<ProductId> ids, string productTypeIdOneC, bool missingOnly = false)
    {
        if (missingOnly)
            Query.Where(p => !ids.Contains(p.Id) && p.ProductTypeIdOneC == productTypeIdOneC);
        else
            Query.Where(p => ids.Contains(p.Id) && p.ProductTypeIdOneC == productTypeIdOneC);

    }
}
