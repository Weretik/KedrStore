using BuildingBlocks.Domain.Abstractions.Errors;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Domain.Attributes;

/// <summary>
/// The attribute's values: a code and a slug are used once within the attribute. Values are added and changed only
/// through the attribute; reads query the value table directly.
/// </summary>
public partial class CatalogAttribute
{
    private readonly List<AttributeValue> _values = [];

    /// <summary>Loaded only by the commands and imports that change values.</summary>
    public IReadOnlyList<AttributeValue> Values => _values;

    public AttributeValue? FindValue(string code)
        => _values.FirstOrDefault(value => string.Equals(value.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));

    public bool IsValueSlugTaken(string slug) => _values.Any(value => value.Slug == slug);

    /// <summary>A value from the attribute dictionary; the import may update it until a person edits it.</summary>
    public DomainResult<AttributeValue> AddValue(string code, string nameUk, string slug, int sortOrder,
        DateTimeOffset now)
    {
        if (CheckNewValue(code, slug) is { } taken)
            return taken;
        var created = AttributeValue.Create(Id, code, nameUk, slug, sortOrder, now);
        if (created.IsSuccess)
            _values.Add(created.Value);
        return created;
    }

    /// <summary>A person adds a value in «Властивості»; it is manual from the start.</summary>
    public DomainResult<AttributeValue> AddManualValue(string code, string nameUk, string? nameRu, string slug,
        int sortOrder, string? swatchHex, long? swatchMediaFileId, DateTimeOffset now)
    {
        if (CheckNewValue(code, slug) is { } taken)
            return taken;
        var created = AttributeValue.CreateManual(Id, code, nameUk, nameRu, slug, sortOrder, swatchHex,
            swatchMediaFileId, now);
        if (created.IsSuccess)
            _values.Add(created.Value);
        return created;
    }

    /// <summary>A person edits a value; the dictionary import no longer changes it.</summary>
    public DomainResult EditValue(AttributeValueId valueId, string nameUk, string? nameRu, int sortOrder,
        string? swatchHex, long? swatchMediaFileId, DateTimeOffset now)
    {
        var value = _values.FirstOrDefault(item => item.Id == valueId);
        return value is null
            ? AttributeErrors.ValueNotFound()
            : value.EditManually(nameUk, nameRu, sortOrder, swatchHex, swatchMediaFileId, now);
    }

    /// <summary>Applies the dictionary name and order unless a person edited the value. Returns whether it changed.</summary>
    public DomainResult<bool> ApplyDictionaryToValue(AttributeValue value, string nameUk, int sortOrder,
        DateTimeOffset now)
    {
        if (!_values.Contains(value))
            return AttributeErrors.ValueNotFound();
        return value.ApplyDictionary(nameUk, sortOrder, now);
    }

    private DomainError? CheckNewValue(string code, string slug)
    {
        if (FindValue(code) is not null)
            return AttributeErrors.ValueCodeTaken();
        return IsValueSlugTaken(slug?.Trim() ?? string.Empty) ? AttributeErrors.ValueSlugTaken() : null;
    }
}
