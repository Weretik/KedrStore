
namespace Catalog.Contracts.Products.Facets;

/// <summary>
/// The attribute filters of a category with counts (spec catalog/006, phase 5). A count is the number of product
/// cards (a variant group is one card) the list would show with that value added to the other attributes' choices.
/// </summary>
public sealed record CategoryFacetsDto(int TotalCards, IReadOnlyList<CategoryFacetDto> Facets);

/// <param name="Display"><c>swatch</c> (colors), <c>button</c> (size, side), or <c>checkbox</c>.</param>
public sealed record CategoryFacetDto(string Code, string Slug, string Name, string Display,
    IReadOnlyList<CategoryFacetValueDto> Values);

/// <summary>A value with its count; values with no cards are left out unless selected.</summary>
public sealed record CategoryFacetValueDto(string Slug, string Name, string? Hex, string? TextureUrl, int Count,
    bool Selected);
