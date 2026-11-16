using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(GetProductBySlugRequest Request) : IQuery<Result<CatalogLookup<ProductBySlugDto>>>;

/// <param name="PriceTypeId">Price type to show; <c>null</c> means the retail type from <c>Catalog:Pricing:RetailPriceTypeId</c>, the same as the lists.</param>
public sealed record GetProductBySlugRequest(string Slug, int? PriceTypeId = null, string Lang = "uk");
