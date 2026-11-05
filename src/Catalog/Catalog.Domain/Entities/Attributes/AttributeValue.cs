using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Attributes;

/// <summary>One value of an attribute, for example «Антична бронза» (code AB) of color.</summary>
public class AttributeValue : BaseEntity<AttributeValueId>, IAggregateRoot
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 160;

    public AttributeId AttributeId { get; private set; }
    public string Code { get; private set; } = null!;
    public string NameUk { get; private set; } = null!;
    public string? NameRu { get; private set; }
    /// <summary>Unique within the attribute; the value part of a filter URL.</summary>
    public string Slug { get; private set; } = null!;
    public int SortOrder { get; private set; }
    /// <summary>Swatch color <c>#RRGGBB</c> for the site filter; empty means a text chip.</summary>
    public string? SwatchHex { get; private set; }
    /// <summary>Media-library picture of a decor texture for the filter swatch; used instead of a color.</summary>
    public long? SwatchMediaFileId { get; private set; }
    public bool IsEditedManually { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private AttributeValue() { }

    public static AttributeValue Create(AttributeId attributeId, string code, string nameUk, string slug, int sortOrder,
        DateTimeOffset now)
        => new()
        {
            AttributeId = attributeId,
            Code = AttributeText.Required(code, CodeMaxLength, nameof(Code)),
            NameUk = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)),
            Slug = AttributeText.Slug(slug),
            SortOrder = sortOrder,
            UpdatedAt = now
        };

    /// <summary>Applies dictionary fields unless a person edited the value. Returns whether anything changed.</summary>
    public bool ApplyDictionary(string nameUk, int sortOrder, DateTimeOffset now)
    {
        if (IsEditedManually)
            return false;

        var name = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk));
        if (NameUk == name && SortOrder == sortOrder)
            return false;

        NameUk = name;
        SortOrder = sortOrder;
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Gives a value without any swatch the built-in color circle (owner decision 2026-11-05: obvious colors are filled
    /// in; decors get a texture picture from a person). Never changes a value a person edited. Returns whether it changed.
    /// </summary>
    public bool ApplyDefaultSwatch(string hex, DateTimeOffset now)
    {
        if (IsEditedManually || SwatchHex is not null || SwatchMediaFileId is not null)
            return false;

        SwatchHex = AttributeText.SwatchHex(hex);
        UpdatedAt = now;
        return true;
    }

    /// <summary>A person adds a value in «Властивості». It is manual from the start.</summary>
    public static AttributeValue CreateManual(AttributeId attributeId, string code, string nameUk, string? nameRu,
        string slug, int sortOrder, string? swatchHex, long? swatchMediaFileId, DateTimeOffset now)
    {
        var value = Create(attributeId, code, nameUk, slug, sortOrder, now);
        value.EditManually(nameUk, nameRu, sortOrder, swatchHex, swatchMediaFileId, now);
        return value;
    }

    /// <summary>A person edits the value; the dictionary import no longer changes it.</summary>
    public void EditManually(string nameUk, string? nameRu, int sortOrder, string? swatchHex, long? swatchMediaFileId,
        DateTimeOffset now)
    {
        NameUk = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk));
        NameRu = AttributeText.Optional(nameRu, NameMaxLength, nameof(NameRu));
        SortOrder = sortOrder;
        SwatchHex = AttributeText.SwatchHex(swatchHex);
        if (swatchMediaFileId is <= 0)
            throw new DomainException(AttributeErrors.SwatchMediaInvalid(swatchMediaFileId.Value));
        SwatchMediaFileId = swatchMediaFileId;
        IsEditedManually = true;
        UpdatedAt = now;
    }
}
