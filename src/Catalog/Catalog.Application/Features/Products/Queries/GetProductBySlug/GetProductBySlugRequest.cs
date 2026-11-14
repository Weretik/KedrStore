namespace Catalog.Application.Features.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugRequest(string Slug, int PriceTypeId = 11, string Lang = "uk");
