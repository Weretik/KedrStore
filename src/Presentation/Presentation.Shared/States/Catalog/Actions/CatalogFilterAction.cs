
using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Application.GetProducts;

namespace Presentation.Shared.States.Catalog;

public static class CatalogFilterAction
{
    public sealed record SetFilter(ProductFilter Filter);
    public sealed record SetSearchTerm(string? SearchTerm);
    public sealed record SetCategory(int? CategoryId);
    public sealed record SetStock(decimal? Stock);
    public sealed record SetProductTypeId(int? ProductTypeId);
}
