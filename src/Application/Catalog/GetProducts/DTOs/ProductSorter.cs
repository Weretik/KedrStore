using Application.Catalog.Shared;

namespace Application.Catalog.GetProducts;

public sealed record ProductSorter(ProductSortKey Key = ProductSortKey.Name, bool Desc = false);
