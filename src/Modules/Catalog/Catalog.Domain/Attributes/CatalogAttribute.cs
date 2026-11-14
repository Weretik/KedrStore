using Catalog.Domain.Attributes.Enums;
using Catalog.Domain.Attributes.ValueObjects;

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

    public static DomainResult<CatalogAttribute> Create(string code, string nameUk, AttributeKind kind, string? unit,
        string slug, bool isFilterable, bool isVariantAxis, int sortOrder, DateTimeOffset now)
    {
        var checks = new DomainChecks();
        var checkedCode = checks.Take(AttributeText.Required(code, CodeMaxLength, nameof(Code)));
        var checkedName = checks.Take(AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)));
        var checkedUnit = checks.Take(AttributeText.Optional(unit, UnitMaxLength, nameof(Unit)));
        var checkedSlug = checks.Take(AttributeText.Slug(slug));
        if (checks.Error is { } error)
            return error;

        return new CatalogAttribute
        {
            Code = checkedCode,
            NameUk = checkedName,
            Kind = kind,
            Unit = checkedUnit,
            Slug = checkedSlug,
            IsFilterable = isFilterable,
            IsVariantAxis = isVariantAxis,
            SortOrder = sortOrder,
            UpdatedAt = now
        };
    }

    /// <summary>Applies dictionary fields unless a person edited the attribute. Returns whether anything changed.</summary>
    public DomainResult<bool> ApplyDictionary(string nameUk, AttributeKind kind, string? unit, int sortOrder,
        DateTimeOffset now)
    {
        if (IsEditedManually)
            return false;

        var checks = new DomainChecks();
        var name = checks.Take(AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)));
        var normalizedUnit = checks.Take(AttributeText.Optional(unit, UnitMaxLength, nameof(Unit)));
        if (checks.Error is { } error)
            return error;
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
    public DomainResult EditManually(string nameUk, string? nameRu, bool isFilterable, int sortOrder, DateTimeOffset now)
    {
        var checks = new DomainChecks();
        var checkedNameUk = checks.Take(AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)));
        var checkedNameRu = checks.Take(AttributeText.Optional(nameRu, NameMaxLength, nameof(NameRu)));
        if (checks.Error is { } error)
            return error;

        NameUk = checkedNameUk;
        NameRu = checkedNameRu;
        IsFilterable = isFilterable;
        SortOrder = sortOrder;
        IsEditedManually = true;
        UpdatedAt = now;
        return DomainResult.Success();
    }
}
