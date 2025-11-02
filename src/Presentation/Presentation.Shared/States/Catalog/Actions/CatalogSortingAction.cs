using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogSortingAction
{
    public sealed record SetSorter(ProductSorter Sorter);
    public sealed record SetSortKey(ProductSortKey Key);
    public sealed record SetSortDesc(bool Desc);
}

