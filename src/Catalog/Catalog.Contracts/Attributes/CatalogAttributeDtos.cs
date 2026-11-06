namespace Catalog.Contracts.Attributes;

/// <summary>An attribute in the «Властивості» list. <c>ProductCount</c>: products that have a value of it.</summary>
public sealed record AdminCatalogAttributeListItemDto(
    int Id,
    string Code,
    string NameUk,
    string? NameRu,
    string Kind,
    string? Unit,
    string Slug,
    bool IsFilterable,
    bool IsVariantAxis,
    int SortOrder,
    bool IsEditedManually,
    int ValueCount,
    int ProductCount);

/// <summary>
/// One value of an attribute. The swatch is a color (<c>SwatchHex</c>) or a decor texture from the media library
/// (<c>SwatchMediaFileId</c>, its URL in <c>SwatchImageUrl</c>); with neither the site shows the name as text.
/// </summary>
public sealed record AdminAttributeValueDto(
    int Id,
    string Code,
    string NameUk,
    string? NameRu,
    string Slug,
    int SortOrder,
    string? SwatchHex,
    long? SwatchMediaFileId,
    string? SwatchImageUrl,
    bool IsEditedManually,
    int ProductCount);

/// <summary>How a value is written in 1C names. Without <c>ValueId</c> the code is not decoded.</summary>
public sealed record AdminAttributeAliasDto(
    int Id,
    string Scope,
    string RawValue,
    int? ValueId,
    bool IsConfirmed,
    string? Note,
    bool IsEditedManually);

public sealed record AdminCatalogAttributeDto(
    AdminCatalogAttributeListItemDto Attribute,
    IReadOnlyList<AdminAttributeValueDto> Values,
    IReadOnlyList<AdminAttributeAliasDto> Aliases);

public sealed record UpdateCatalogAttributeRequest(string NameUk, string? NameRu, bool IsFilterable, int SortOrder);

/// <summary>
/// A new attribute a person adds in «Ознаки». <c>Kind</c>: <c>enum</c> (a list of values) or <c>number</c> (a number in
/// <c>Unit</c>). Code and slug are taken from <c>Code</c>/<c>Slug</c> or made from the name. <c>IsVariantAxis</c>: the
/// products of a model may differ by it (it is offered as a model switcher).
/// </summary>
public sealed record CreateCatalogAttributeRequest(
    string NameUk,
    string? NameRu,
    string Kind,
    string? Unit,
    string? Code,
    string? Slug,
    bool IsFilterable,
    bool IsVariantAxis,
    int SortOrder);

/// <summary>A new value; its code and slug are taken from <c>Code</c>/<c>Slug</c> or made from the name.</summary>
public sealed record CreateAttributeValueRequest(
    string NameUk,
    string? NameRu,
    string? Code,
    string? Slug,
    int SortOrder,
    string? SwatchHex,
    long? SwatchMediaFileId);

public sealed record UpdateAttributeValueRequest(
    string NameUk,
    string? NameRu,
    int SortOrder,
    string? SwatchHex,
    long? SwatchMediaFileId);

/// <summary>
/// A code in 1C names nobody has decoded or confirmed («Не розшифровані коди»). <c>ProductCount</c>: products whose
/// review mentions the code.
/// </summary>
public sealed record AdminUnresolvedCodeDto(
    int AliasId,
    int AttributeId,
    string AttributeName,
    string Scope,
    string RawValue,
    int? ValueId,
    string? ValueName,
    bool IsConfirmed,
    string? Note,
    int ProductCount);

/// <summary>
/// The owner's decoding. With <c>ApplyToAll</c> (the default) <c>ValueId</c> decodes the code for all products: a value
/// of the code's attribute, or <c>null</c> when the code means nothing to filter by; without it the code stays
/// undecided for all products. <c>Categories</c>, when given, replaces the code's own decodings per category
/// (spec catalog/008); a category left out follows the decoding for all products.
/// </summary>
public sealed record DecideAttributeCodeRequest(
    int? ValueId,
    bool ApplyToAll = true,
    IReadOnlyList<DecideAttributeCodeCategoryRequest>? Categories = null);

/// <summary>The decoding of a code in one category: a value, or <c>null</c> when it means nothing there.</summary>
public sealed record DecideAttributeCodeCategoryRequest(int CategoryId, int? ValueId);

/// <summary>
/// Where a code occurs (spec catalog/008): on-site products whose 1C names contain it, by category, with the code's own
/// decoding in each category.
/// </summary>
public sealed record AdminCodeProductsDto(int AliasId, string RawValue, IReadOnlyList<AdminCodeCategoryDto> Categories);

/// <summary>
/// A category where the code occurs. <c>HasOwnValue</c>: the category has its own decoding (<c>ValueId</c>, or
/// <c>null</c> when the code means nothing there); otherwise it follows the decoding for all products.
/// </summary>
public sealed record AdminCodeCategoryDto(
    int CategoryId,
    string CategoryPath,
    bool HasOwnValue,
    int? ValueId,
    string? ValueName,
    IReadOnlyList<AdminCodeProductDto> Products);

/// <summary>A product whose original 1C name contains the code.</summary>
public sealed record AdminCodeProductDto(int ProductId, string Name, string Photo);

/// <summary>
/// The result of a decoding. <c>ValuesConfirmed</c>: product values that became confirmed now; products with a code
/// decoded for the first time get their value at the next recognition run.
/// </summary>
public sealed record DecideAttributeCodeResponse(AdminUnresolvedCodeDto Code, int ValuesConfirmed);

/// <summary>An attribute offered as a filter of a category, in order.</summary>
public sealed record AdminCategoryAttributeFilterDto(
    int AttributeId,
    string Code,
    string Name,
    bool IsFilterable,
    int SortOrder,
    bool IsEditedManually,
    int ProductCount);

public sealed record SetCategoryAttributeFilterRequest(int AttributeId, bool IsFilterable, int SortOrder);

/// <summary>The whole filter list of a category; attributes left out stay as they are.</summary>
public sealed record SetCategoryAttributeFiltersRequest(IReadOnlyList<SetCategoryAttributeFilterRequest> Filters);

/// <summary>A review reason with the number of on-site products that need review for it (filter «За причиною»).</summary>
public sealed record AdminProblemReasonDto(string Code, int ProductCount);
