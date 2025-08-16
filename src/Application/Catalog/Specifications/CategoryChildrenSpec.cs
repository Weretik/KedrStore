namespace Application.Catalog.Specifications;

public sealed class CategoryChildrenSpec : Specification<Category>
{
    public CategoryChildrenSpec(CategoryId parentId)
    {
        Query.Where(c => c.ParentCategoryId == parentId)
            .AsNoTracking();
    }
}
