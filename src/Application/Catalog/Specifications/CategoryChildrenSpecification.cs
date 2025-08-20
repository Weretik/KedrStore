namespace Application.Catalog.Specifications;

public sealed class CategoryChildrenSpecification : Specification<Category>
{
    public CategoryChildrenSpecification(CategoryId parentId)
    {
        Query.Where(c => c.ParentCategoryId == parentId)
            .AsNoTracking();
    }
}
