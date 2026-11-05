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
}
