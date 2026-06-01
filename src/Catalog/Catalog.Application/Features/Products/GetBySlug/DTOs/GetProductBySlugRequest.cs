namespace Catalog.Application.Features.Products.GetBySlug.DTOs;

public sealed record GetProductBySlugRequest(string Slug, int PriceTypeId = 11, string Lang = "uk");
