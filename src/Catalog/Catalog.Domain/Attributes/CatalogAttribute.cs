namespace Catalog.Domain.Attributes;

/// <summary>
/// A product property such as color, size, or side. Loaded from the attribute dictionary; once a person edits it
/// in the Admin, the dictionary import no longer changes it.
/// </summary>
public class CatalogAttribute : BaseEntity<AttributeId>, IAggregateRoot
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 120;
    public const int UnitMaxLength = 16;

    public string Code { get; private set; } = null!;
    public string NameUk { get; private set; } = null!;
    public string? NameRu { get; private set; }
    public AttributeKind Kind { get; private set; }
    public string? Unit { get; private set; }
    /// <summary>The attribute part of a filter URL, for example <c>kolir</c> in <c>kolir-antychna-bronza</c>.</summary>
    public string Slug { get; private set; } = null!;
    public bool IsFilterable { get; private set; }
    /// <summary>Products of one group that differ by this attribute are variants of each other.</summary>
    public bool IsVariantAxis { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsEditedManually { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CatalogAttribute() { }

    public static CatalogAttribute Create(string code, string nameUk, AttributeKind kind, string? unit, string slug,
        bool isFilterable, bool isVariantAxis, int sortOrder, DateTimeOffset now)
        => new()
        {
            Code = AttributeText.Required(code, CodeMaxLength, nameof(Code)),
            NameUk = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)),
            Kind = kind,
            Unit = AttributeText.Optional(unit, UnitMaxLength, nameof(Unit)),
            Slug = AttributeText.Slug(slug),
            IsFilterable = isFilterable,
            IsVariantAxis = isVariantAxis,
            SortOrder = sortOrder,
            UpdatedAt = now
        };

    /// <summary>Applies dictionary fields unless a person edited the attribute. Returns whether anything changed.</summary>
    public bool ApplyDictionary(string nameUk, AttributeKind kind, string? unit, int sortOrder, DateTimeOffset now)
    {
        if (IsEditedManually)
            return false;

        var name = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk));
        var normalizedUnit = AttributeText.Optional(unit, UnitMaxLength, nameof(Unit));
        if (NameUk == name && Kind == kind && Unit == normalizedUnit && SortOrder == sortOrder)
            return false;

        NameUk = name;
        Kind = kind;
        Unit = normalizedUnit;
        SortOrder = sortOrder;
        UpdatedAt = now;
        return true;
    }

    /// <summary>A person edits the attribute in «Властивості»; the dictionary import no longer changes it.</summary>
    public void EditManually(string nameUk, string? nameRu, bool isFilterable, int sortOrder, DateTimeOffset now)
    {
        NameUk = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk));
        NameRu = AttributeText.Optional(nameRu, NameMaxLength, nameof(NameRu));
        IsFilterable = isFilterable;
        SortOrder = sortOrder;
        IsEditedManually = true;
        UpdatedAt = now;
    }
}
