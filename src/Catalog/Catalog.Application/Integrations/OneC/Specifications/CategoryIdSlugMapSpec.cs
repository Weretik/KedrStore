using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class CategoryIdSlugMapSpec: Specification<ProductCategory, CategoryIdSlugRow>
{
    public CategoryIdSlugMapSpec()
    {
        Query
            .AsNoTracking()
            .Select(c => new CategoryIdSlugRow(c.Id, c.Slug));
    }
}
