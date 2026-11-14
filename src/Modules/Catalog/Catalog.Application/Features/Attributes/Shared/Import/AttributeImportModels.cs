using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.Enums;

namespace Catalog.Application.Features.Attributes.Shared.Import;

/// <summary>An attribute from the dictionary file (or a built-in one such as size), before it is stored.</summary>
public sealed record AttributeDefinition(
    string Code, string NameUk, AttributeKind Kind, string? Unit, string Slug,
    bool IsFilterable, bool IsVariantAxis, int SortOrder);

/// <summary>A list value of an attribute.</summary>
public sealed record AttributeValueDefinition(string AttributeCode, string Code, string NameUk, int SortOrder);

/// <summary>A spelling of a value in 1C names; <see cref="ValueCode"/> is null for a known but undecoded code.</summary>
public sealed record AttributeAliasDefinition(
    string AttributeCode, string Scope, string RawValue, string? ValueCode, bool IsConfirmed, string? Note);

/// <summary>Everything the dictionary stage stores: attributes, values, and aliases.</summary>
public sealed record AttributeDictionary(
    IReadOnlyList<AttributeDefinition> Attributes,
    IReadOnlyList<AttributeValueDefinition> Values,
    IReadOnlyList<AttributeAliasDefinition> Aliases);

/// <summary>One recognized attribute value of a product: a list value code or a number.</summary>
public sealed record RecognizedValue(string AttributeCode, int Position, string? ValueCode, decimal? Number, bool IsConfirmed);

/// <summary>One recognized reason for review.</summary>
public sealed record RecognizedReason(string Code, string? Text);

/// <summary>The recognition result for one product, read from the CSV or produced by the parser.</summary>
public sealed record RecognizedProduct(
    int ProductId, int LineNumber, string BaseName,
    IReadOnlyList<RecognizedValue> Values, IReadOnlyList<RecognizedReason> Reasons);
