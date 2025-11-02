using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogFilterAction
{
    public sealed record SetFilter(ProductFilter Filter);
    public sealed record SetSearchTerm(string? Value);
    public sealed record SetCategory(ProductCategoryId? CategoryId);
    public sealed record SetStock(decimal? Value);
}
