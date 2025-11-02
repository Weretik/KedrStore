using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogLoadAction
{
    public sealed record Load;
    public sealed record LoadSuccess(GetProductsQueryResult<ProductDto> QueryResult);
    public sealed record LoadFailure(string Error);
}
