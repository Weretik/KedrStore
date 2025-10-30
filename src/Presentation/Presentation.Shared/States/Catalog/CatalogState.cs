using Application.Catalog.GetProducts;
using Application.Catalog.Shared.DTOs;

namespace Presentation.Shared.States.Catalog;

[FeatureState]
public sealed record CatalogState(
    CatalogParams Params,
    bool IsLoading = false,
    string? Error = null,
    PaginationResult<ProductDto>? PageList = null )
{
    private CatalogState() : this(new CatalogParams(), false, null, null) { }
}
