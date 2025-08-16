namespace Application.Catalog.Specifications;

public class ProductsByCategorySpec : Specification<Product>
{
    public ProductsByCategorySpec(CategoryId categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId)
            .AsNoTracking();
    }
}
