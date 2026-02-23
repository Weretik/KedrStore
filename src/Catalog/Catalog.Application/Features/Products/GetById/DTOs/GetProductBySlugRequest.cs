namespace Catalog.Application.Features.Products.GetById.DTOs;

public sealed record GetProductBySlugRequest(string Slug, int PriceTypeId = 11);
