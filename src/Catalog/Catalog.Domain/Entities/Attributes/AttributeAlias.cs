using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Attributes;

/// <summary>
/// How an attribute value is written in 1C names (<c>AB</c>, «антична бронза»). An alias without a value is a known
/// code nobody has decoded yet; products that contain it need review.
/// </summary>
public class AttributeAlias : BaseEntity<AttributeAliasId>, IAggregateRoot
{
    public const int ScopeMaxLength = 64;
    public const int RawValueMaxLength = 160;
    public const int NoteMaxLength = 1000;

    public AttributeId AttributeId { get; private set; }
    /// <summary>Where in a name the spelling applies (dictionary scope, for example <c>general</c>).</summary>
    public string Scope { get; private set; } = null!;
    public string RawValue { get; private set; } = null!;
    public AttributeValueId? ValueId { get; private set; }
    /// <summary>False while the decoding was picked by an assistant and the owner has not confirmed it.</summary>
    public bool IsConfirmed { get; private set; }
    public string? Note { get; private set; }
    public bool IsEditedManually { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private AttributeAlias() { }

    public static AttributeAlias Create(AttributeId attributeId, string scope, string rawValue, AttributeValueId? valueId,
        bool isConfirmed, string? note, DateTimeOffset now)
        => new()
        {
            AttributeId = attributeId,
            Scope = AttributeText.Required(scope, ScopeMaxLength, nameof(Scope)),
            RawValue = AttributeText.Required(rawValue, RawValueMaxLength, nameof(RawValue)),
            ValueId = valueId,
            IsConfirmed = isConfirmed,
            Note = AttributeText.Optional(note, NoteMaxLength, nameof(Note)),
            UpdatedAt = now
        };

    /// <summary>Applies dictionary fields unless a person edited the alias. Returns whether anything changed.</summary>
    public bool ApplyDictionary(AttributeValueId? valueId, bool isConfirmed, string? note, DateTimeOffset now)
    {
        if (IsEditedManually)
            return false;

        var normalizedNote = AttributeText.Optional(note, NoteMaxLength, nameof(Note));
        if (ValueId == valueId && IsConfirmed == isConfirmed && Note == normalizedNote)
            return false;

        ValueId = valueId;
        IsConfirmed = isConfirmed;
        Note = normalizedNote;
        UpdatedAt = now;
        return true;
    }
}
