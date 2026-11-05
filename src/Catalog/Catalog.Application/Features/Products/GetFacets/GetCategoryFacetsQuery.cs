using Catalog.Contracts.Products.Facets;
using Catalog.Contracts.Products.GetList;

namespace Catalog.Application.Features.Products.GetFacets;

/// <summary>The attribute filters of a category with counts for the same request as the product list.</summary>
public sealed record GetCategoryFacetsQuery(GetProductsRequest Request) : IQuery<Result<CategoryFacetsDto>>;
