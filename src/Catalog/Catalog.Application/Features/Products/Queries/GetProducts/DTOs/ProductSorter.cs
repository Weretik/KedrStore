using Catalog.Application.Shared;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed record ProductSorter(ProductSortKey Key = ProductSortKey.Name, bool Desc = false);
