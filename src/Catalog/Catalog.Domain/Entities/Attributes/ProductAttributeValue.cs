using Catalog.Domain.Enums;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Attributes;

/// <summary>
/// One attribute value of a product. <see cref="Position"/> orders several values of one attribute (the second color
/// of a two-tone handle is position 1). Values set by a person (<see cref="AttributeValueSource.Manual"/>) or locked
/// are never changed by the import, the parser, or the 1C sync.
/// </summary>
public class ProductAttributeValue : IAggregateRoot
{
    public const int MaxPosition = 20;

    public ProductId ProductId { get; private set; }
    public AttributeId AttributeId { get; private set; }
    public short Position { get; private set; }
    public AttributeValueId? ValueId { get; private set; }
    public decimal? NumberValue { get; private set; }
    public AttributeValueSource Source { get; private set; }
    public bool IsLocked { get; private set; }
    /// <summary>False when the value came from a decoding the owner has not confirmed; such values stay off filters.</summary>
    public bool IsConfirmed { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Whether automatic sources (import, parser, 1C sync) may change or remove this value.</summary>
    public bool IsAutomatic => Source != AttributeValueSource.Manual && !IsLocked;

    private ProductAttributeValue() { }

    public static ProductAttributeValue CreateAutomatic(ProductId productId, AttributeId attributeId, int position,
        AttributeValueId? valueId, decimal? numberValue, bool isConfirmed, AttributeValueSource source, DateTimeOffset now)
    {
        if (source == AttributeValueSource.Manual)
            throw new ArgumentOutOfRangeException(nameof(source), source, "Manual values are set through SetManually.");

        var value = new ProductAttributeValue
        {
            ProductId = productId,
            AttributeId = attributeId,
            Position = CheckPosition(position)
        };
        value.Set(valueId, numberValue, isConfirmed, source, now);
        return value;
    }

    /// <summary>Applies an automatic result. Returns false and changes nothing for manual or locked values.</summary>
    public bool ApplyAutomatic(AttributeValueId? valueId, decimal? numberValue, bool isConfirmed,
        AttributeValueSource source, DateTimeOffset now)
    {
        if (!IsAutomatic || source == AttributeValueSource.Manual)
            return false;

        if (ValueId == valueId && NumberValue == numberValue && IsConfirmed == isConfirmed && Source == source)
            return false;

        Set(valueId, numberValue, isConfirmed, source, now);
        return true;
    }

    /// <summary>A person sets the value. It becomes manual and locked: automatic sources no longer change it.</summary>
    public static ProductAttributeValue CreateManual(ProductId productId, AttributeId attributeId, int position,
        AttributeValueId? valueId, decimal? numberValue, DateTimeOffset now)
    {
        var value = new ProductAttributeValue
        {
            ProductId = productId,
            AttributeId = attributeId,
            Position = CheckPosition(position)
        };
        value.SetManually(valueId, numberValue, now);
        return value;
    }

    /// <summary>A person changes the value. It becomes manual and locked.</summary>
    public void SetManually(AttributeValueId? valueId, decimal? numberValue, DateTimeOffset now)
    {
        Set(valueId, numberValue, true, AttributeValueSource.Manual, now);
        IsLocked = true;
    }

    private void Set(AttributeValueId? valueId, decimal? numberValue, bool isConfirmed, AttributeValueSource source,
        DateTimeOffset now)
    {
        if (valueId is null && numberValue is null)
            throw new DomainException(AttributeErrors.ValueOrNumberRequired());

        ValueId = valueId;
        NumberValue = numberValue;
        IsConfirmed = isConfirmed;
        Source = source;
        UpdatedAt = now;
    }

    private static short CheckPosition(int position)
    {
        if (position is < 0 or > MaxPosition)
            throw new DomainException(AttributeErrors.PositionInvalid(position));

        return (short)position;
    }
}
