using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Integrations.OneC.Specifications;

/// <summary>Every product of a 1C root, including soft-deleted ones, tracked for the import diff.</summary>
public sealed class ProductsByRootSpec : Specification<Product>
{
    public ProductsByRootSpec(string productTypeIdOneC)
    {
        Query
            .IgnoreQueryFilters()
            .Where(product => product.ProductTypeIdOneC == productTypeIdOneC);
    }
}
