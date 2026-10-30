using Catalog.Domain.Entities;

namespace Catalog.Application.Integrations.OneC.Specifications;

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
