namespace Catalog.Contracts.Attributes;

/// <summary>Why a product needs review: a stable code and the recognizer's explanation, if any.</summary>
public sealed record AdminAttributeProblemDto(string Code, string? Text);

/// <summary>
/// The review of one product. <c>State</c>: <c>clear</c>, <c>needs_review</c>, or <c>accepted</c> («Прийняти як є»).
/// </summary>
public sealed record AdminProductReviewDto(
    string State,
    IReadOnlyList<AdminAttributeProblemDto> Reasons,
    string? AcceptedBy,
    DateTimeOffset? AcceptedAt);

/// <summary>
/// One value of a product attribute. <c>Source</c>: <c>import</c>, <c>parser</c>, or <c>manual</c>. A locked value is
/// never changed by automation; <c>HasAutomatic</c> tells whether «Повернути автоматичне» brings a value back.
/// </summary>
public sealed record AdminProductAttributeValueDto(
    int Position,
    int? ValueId,
    string? ValueName,
    decimal? Number,
    string Source,
    bool IsLocked,
    bool IsConfirmed,
    bool HasAutomatic);

/// <summary>One attribute of a product with its values in position order. <c>Kind</c>: <c>enum</c>, <c>number</c>, <c>size</c>.</summary>
public sealed record AdminProductAttributeDto(
    int AttributeId,
    string Code,
    string Name,
    string Kind,
    string? Unit,
    bool IsVariantAxis,
    IReadOnlyList<AdminProductAttributeValueDto> Values);

/// <summary>
/// The «Властивості» panel of a product: the original 1C name (never changed), the base name used for grouping,
/// the review, and the recognized attributes.
/// </summary>
public sealed record AdminProductAttributesDto(
    int ProductId,
    string OriginalName,
    string? BaseName,
    AdminProductReviewDto Review,
    IReadOnlyList<AdminProductAttributeDto> Attributes);

/// <summary>One value a person sets: a list value for <c>enum</c>/<c>size</c> attributes, a number for <c>number</c>.</summary>
public sealed record SetProductAttributeValueRequest(int? ValueId, decimal? Number);

/// <summary>The values of one attribute in position order; they become manual and locked.</summary>
public sealed record SetProductAttributeRequest(IReadOnlyList<SetProductAttributeValueRequest> Values);
