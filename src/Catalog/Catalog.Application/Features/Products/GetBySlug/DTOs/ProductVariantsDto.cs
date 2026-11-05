using Catalog.Contracts.Products.GetList;

namespace Catalog.Application.Features.Products.GetBySlug.DTOs;

/// <summary>
/// The variant group of a product page (spec catalog/006, phase 5): the axes that differ inside the group and every
/// variant on the site. Each variant stays a product with its own page, code, price, and stock.
/// </summary>
/// <param name="MainProductSlug">Slug of the group's main product on the site; the canonical page of the group.</param>
public sealed record ProductVariantsDto(string GroupName, string GroupSlug, string? MainProductSlug,
    IReadOnlyList<VariantAxisDto> Axes, IReadOnlyList<VariantItemDto> Items);

/// <param name="Display"><c>swatch</c> for colors, <c>button</c> otherwise.</param>
public sealed record VariantAxisDto(string Code, string Slug, string Name, string Display,
    IReadOnlyList<VariantAxisValueDto> Values);

/// <summary>A value of an axis; a two-color value (main and second color) has two swatches.</summary>
public sealed record VariantAxisValueDto(string Key, string Name, IReadOnlyList<ProductSwatchDto> Swatches);

/// <param name="Values">Axis code → value key; an axis the variant has no value for is missing.</param>
public sealed record VariantItemDto(int Id, string ProductSlug, string Name, string Photo, decimal? Price,
    bool InStock, bool IsCurrent, bool IsMain, IReadOnlyDictionary<string, string> Values);

/// <summary>A row of «Характеристики»: confirmed values of one attribute, joined in position order.</summary>
public sealed record ProductCharacteristicDto(string Code, string Name, string Value);
