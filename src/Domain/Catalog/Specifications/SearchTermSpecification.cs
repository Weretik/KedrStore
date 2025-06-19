namespace Domain.Catalog.Specifications;

public class SearchTermSpecification(string? searchTerm)
    : SpecificationBase<Product>
{
    private readonly string? _searchTerm = searchTerm?.Trim().ToLower();

    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => string.IsNullOrEmpty(_searchTerm) ||
                         product.Name.ToLower().Contains(_searchTerm) ||
                         product.Id.ToString().ToLower().Contains(_searchTerm);
    }
}
