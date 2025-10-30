using Application.Catalog.GetProducts;
using Application.Catalog.Shared.DTOs;

namespace Presentation.Shared.States.Catalog;

public static class CatalogActions
{
    public sealed record SetParams(CatalogParams Params);
    public sealed record ResetParams;
    public sealed record SetPageNumber(int PageNumber);

    public sealed record Load;
    public sealed record LoadSuccess(PaginationResult<ProductDto> PageList);
    public sealed record LoadFailure(string Error);
}
