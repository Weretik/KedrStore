namespace Presentation.Shared.States.Catalog;

[FeatureState]
public sealed record CatalogState(
    CatalogParams Params,
    bool IsLoading = false,
    string? Error = null,
    PaginatedList<ProductDto>? PageList = null )
{
    private CatalogState() : this(new CatalogParams(), false, null, null) { }
}
