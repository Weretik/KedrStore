using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Application.Features.Shared;
using Catalog.Application.GetProducts;
using Catalog.Application.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogLoadAction
{
    public sealed record Load;
    public sealed record Reset;
    public sealed record LoadSuccess(GetProductsQueryResult<ProductDto> QueryResult);
    public sealed record LoadFailure(string Error);
}
