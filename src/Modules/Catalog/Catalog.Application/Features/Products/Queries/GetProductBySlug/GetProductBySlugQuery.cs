using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(GetProductBySlugRequest Request) : IQuery<Result<CatalogLookup<ProductBySlugDto>>>;

public sealed record GetProductBySlugRequest(string Slug, int PriceTypeId = 11, string Lang = "uk");
