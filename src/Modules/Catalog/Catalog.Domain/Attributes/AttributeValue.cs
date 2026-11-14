namespace Catalog.Domain.Attributes;

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

    public static DomainResult<AttributeValue> Create(AttributeId attributeId, string code, string nameUk, string slug,
        int sortOrder, DateTimeOffset now)
    {
        var checks = new DomainChecks();
        var checkedCode = checks.Take(AttributeText.Required(code, CodeMaxLength, nameof(Code)));
        var checkedName = checks.Take(AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)));
        var checkedSlug = checks.Take(AttributeText.Slug(slug));
        if (checks.Error is { } error)
            return error;

        return new AttributeValue
        {
            AttributeId = attributeId,
            Code = checkedCode,
            NameUk = checkedName,
            Slug = checkedSlug,
            SortOrder = sortOrder,
            UpdatedAt = now
        };
    }

    /// <summary>Applies dictionary fields unless a person edited the value. Returns whether anything changed.</summary>
    public DomainResult<bool> ApplyDictionary(string nameUk, int sortOrder, DateTimeOffset now)
    {
        if (IsEditedManually)
            return false;

        var checkedName = AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk));
        if (checkedName.IsFailure)
            return checkedName.Error!;
        var name = checkedName.Value;
        if (NameUk == name && SortOrder == sortOrder)
            return false;

        NameUk = name;
        SortOrder = sortOrder;
        UpdatedAt = now;
        return true;
    }

    /// <summary>A person adds a value in «Властивості». It is manual from the start.</summary>
    public static DomainResult<AttributeValue> CreateManual(AttributeId attributeId, string code, string nameUk,
        string? nameRu, string slug, int sortOrder, string? swatchHex, long? swatchMediaFileId, DateTimeOffset now)
    {
        var created = Create(attributeId, code, nameUk, slug, sortOrder, now);
        if (created.IsFailure)
            return created;
        var edited = created.Value.EditManually(nameUk, nameRu, sortOrder, swatchHex, swatchMediaFileId, now);
        return edited.IsFailure ? edited.Error! : created;
    }

    /// <summary>A person edits the value; the dictionary import no longer changes it.</summary>
    public DomainResult EditManually(string nameUk, string? nameRu, int sortOrder, string? swatchHex,
        long? swatchMediaFileId, DateTimeOffset now)
    {
        var checks = new DomainChecks();
        var checkedNameUk = checks.Take(AttributeText.Required(nameUk, NameMaxLength, nameof(NameUk)));
        var checkedNameRu = checks.Take(AttributeText.Optional(nameRu, NameMaxLength, nameof(NameRu)));
        var checkedHex = checks.Take(AttributeText.SwatchHex(swatchHex));
        if (swatchMediaFileId is <= 0)
            checks.Check(AttributeErrors.SwatchMediaInvalid());
        if (checks.Error is { } error)
            return error;

        NameUk = checkedNameUk;
        NameRu = checkedNameRu;
        SortOrder = sortOrder;
        SwatchHex = checkedHex;
        SwatchMediaFileId = swatchMediaFileId;
        IsEditedManually = true;
        UpdatedAt = now;
        return DomainResult.Success();
    }
}
