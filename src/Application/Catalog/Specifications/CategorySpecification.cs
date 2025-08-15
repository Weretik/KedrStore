namespace Domain.Catalog.Specifications;

public class CategorySpecification(CategoryId? categoryId)
    : SpecificationBase<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => categoryId == null ||
                          product.CategoryId == categoryId;
    }
}
