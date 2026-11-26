namespace Catalog.Application.Features.OneCSync.Sync.Categories;

/// <summary>
/// ID and name of the active 1C categories of a root, for product → category mapping. A manual category is never a
/// 1C folder (spec catalog/011), so it does not take part.
/// </summary>
public sealed class ActiveCategoryNamesByRootSpec : Specification<ProductCategory, CategoryNameRow>
{
    public ActiveCategoryNamesByRootSpec(string productTypeIdOneC)
    {
        Query
            .AsNoTracking()
            .Where(category => category.ProductTypeIdOneC == productTypeIdOneC && !category.IsManual);

        Query.Select(category => new CategoryNameRow(category.Id.Value, category.Name));
    }
}

public sealed record CategoryNameRow(int Id, string Name);
