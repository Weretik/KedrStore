namespace Catalog.Application.Features.OneCSync.Sync;

/// <summary>Every product of a 1C root, including soft-deleted ones, tracked for the import diff.</summary>
public sealed class ProductsByRootSpec : Specification<Product>
{
    public ProductsByRootSpec(string productTypeIdOneC)
    {
        Query
            .IgnoreQueryFilters()
            .Where(product => product.ProductTypeIdOneC == productTypeIdOneC);
    }
}
