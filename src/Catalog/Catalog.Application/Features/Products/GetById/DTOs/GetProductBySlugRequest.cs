// Licensed to KedrStore Development Team under MIT License.

namespace Catalog.Application.Features.Products.GetById.DTOs;

public record GetProductBySlugRequest
{
    public string Slug { get; init; } = null!;
    public int PriceTypeId { get; init; } = 10;
}
