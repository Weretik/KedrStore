namespace Application.Catalog.Specifications;

public class ProductsByCategorySpecification : Specification<Product>
{
    public ProductsByCategorySpecification(CategoryId categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId)
            .AsNoTracking();
    }
}
