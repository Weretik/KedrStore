using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class CategoryIdSlugMapSpec: Specification<ProductCategory, CategoryIdAndNameRow>
{
    public CategoryIdSlugMapSpec()
    {
        Query
            .AsNoTracking()
            .Select(c => new CategoryIdAndNameRow(c.Id, c.Name));
    }
}
