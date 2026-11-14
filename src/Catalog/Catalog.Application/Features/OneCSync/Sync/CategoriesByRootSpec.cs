using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.OneCSync.Sync;

/// <summary>Every category of a 1C root, including soft-deleted ones, tracked for the import diff.</summary>
public sealed class CategoriesByRootSpec : Specification<ProductCategory>
{
    public CategoriesByRootSpec(string productTypeIdOneC)
    {
        Query
            .IgnoreQueryFilters()
            .Where(category => category.ProductTypeIdOneC == productTypeIdOneC);
    }
}
