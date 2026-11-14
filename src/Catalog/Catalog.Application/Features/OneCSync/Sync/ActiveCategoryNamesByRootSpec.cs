using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.OneCSync.Sync;

/// <summary>ID and name of the active categories of a 1C root, for product → category mapping.</summary>
public sealed class ActiveCategoryNamesByRootSpec : Specification<ProductCategory, CategoryNameRow>
{
    public ActiveCategoryNamesByRootSpec(string productTypeIdOneC)
    {
        Query
            .AsNoTracking()
            .Where(category => category.ProductTypeIdOneC == productTypeIdOneC);

        Query.Select(category => new CategoryNameRow(category.Id.Value, category.Name));
    }
}

public sealed record CategoryNameRow(int Id, string Name);
