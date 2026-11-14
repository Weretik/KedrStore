using Catalog.Domain.Products;
using Catalog.Domain.Attributes.Enums;
using Catalog.Domain.Attributes.ValueObjects;
namespace Catalog.Domain.Attributes.ProductValues;

/// <summary>
/// One attribute value of a product. <see cref="Position"/> orders several values of one attribute (the second color
/// of a two-tone handle is position 1). Values set by a person (<see cref="AttributeValueSource.Manual"/>) or locked
/// are never changed by the import, the parser, or the 1C sync. While a value is manual, the latest automatic result
/// for its position is kept in the <c>Automatic*</c> fields, so «Повернути автоматичне» restores it.
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

    /// <summary>The automatic result kept while the value is manual; empty when automation has none.</summary>
    public AttributeValueId? AutomaticValueId { get; private set; }
    public decimal? AutomaticNumberValue { get; private set; }
    public bool AutomaticIsConfirmed { get; private set; }
    public AttributeValueSource? AutomaticSource { get; private set; }

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
            Position = AttributeText.Guarded(CheckPosition(position), nameof(position))
        };
        AttributeText.Guarded(CheckValue(valueId, numberValue), nameof(valueId));
        value.Set(valueId, numberValue, isConfirmed, source, now);
        return value;
    }

    /// <summary>
    /// Applies an automatic result. Returns false and changes nothing visible for manual or locked values; for them the
    /// result is only kept for «Повернути автоматичне».
    /// </summary>
    public bool ApplyAutomatic(AttributeValueId? valueId, decimal? numberValue, bool isConfirmed,
        AttributeValueSource source, DateTimeOffset now)
    {
        if (source == AttributeValueSource.Manual)
            return false;

        if (!IsAutomatic)
        {
            AttributeText.Guarded(CheckValue(valueId, numberValue), nameof(valueId));
            AutomaticValueId = valueId;
            AutomaticNumberValue = numberValue;
            AutomaticIsConfirmed = isConfirmed;
            AutomaticSource = source;
            return false;
        }

        if (ValueId == valueId && NumberValue == numberValue && IsConfirmed == isConfirmed && Source == source)
            return false;

        AttributeText.Guarded(CheckValue(valueId, numberValue), nameof(valueId));
        Set(valueId, numberValue, isConfirmed, source, now);
        return true;
    }

    /// <summary>A person sets the value. It becomes manual and locked: automatic sources no longer change it.</summary>
    public static DomainResult<ProductAttributeValue> CreateManual(ProductId productId, AttributeId attributeId,
        int position, AttributeValueId? valueId, decimal? numberValue, DateTimeOffset now)
    {
        var checkedPosition = CheckPosition(position);
        if (checkedPosition.IsFailure)
            return checkedPosition.Error!;

        var value = new ProductAttributeValue
        {
            ProductId = productId,
            AttributeId = attributeId,
            Position = checkedPosition.Value
        };
        var set = value.SetManually(valueId, numberValue, now);
        return set.IsFailure ? set.Error! : value;
    }

    /// <summary>
    /// A person changes the value. It becomes manual and locked; the automatic value it replaces is kept.
    /// Returns whether anything changed.
    /// </summary>
    public DomainResult<bool> SetManually(AttributeValueId? valueId, decimal? numberValue, DateTimeOffset now)
    {
        if (!IsAutomatic && ValueId == valueId && NumberValue == numberValue)
            return false;

        var checkedValue = CheckValue(valueId, numberValue);
        if (checkedValue.IsFailure)
            return checkedValue.Error!;

        if (IsAutomatic && Source != default)
        {
            AutomaticValueId = ValueId;
            AutomaticNumberValue = NumberValue;
            AutomaticIsConfirmed = IsConfirmed;
            AutomaticSource = Source;
        }

        Set(valueId, numberValue, true, AttributeValueSource.Manual, now);
        IsLocked = true;
        return true;
    }

    /// <summary>
    /// «Повернути автоматичне»: brings back the kept automatic value and unlocks the row. Returns false when automation
    /// has no value for this position; the caller removes the row then.
    /// </summary>
    public bool RestoreAutomatic(DateTimeOffset now)
    {
        if (AutomaticSource is not { } source)
            return false;

        Set(AutomaticValueId, AutomaticNumberValue, AutomaticIsConfirmed, source, now);
        IsLocked = false;
        ClearAutomatic();
        return true;
    }

    /// <summary>Automation no longer has a value for this manual position.</summary>
    public void ClearAutomatic()
    {
        AutomaticValueId = null;
        AutomaticNumberValue = null;
        AutomaticIsConfirmed = false;
        AutomaticSource = null;
    }

    /// <summary>The owner confirmed the decoding behind this automatic value.</summary>
    public bool Confirm(DateTimeOffset now)
    {
        if (IsConfirmed || !IsAutomatic)
            return false;

        IsConfirmed = true;
        UpdatedAt = now;
        return true;
    }

    private void Set(AttributeValueId? valueId, decimal? numberValue, bool isConfirmed, AttributeValueSource source,
        DateTimeOffset now)
    {
        ValueId = valueId;
        NumberValue = numberValue;
        IsConfirmed = isConfirmed;
        Source = source;
        UpdatedAt = now;
    }

    private static DomainResult CheckValue(AttributeValueId? valueId, decimal? numberValue)
        => valueId is null && numberValue is null ? AttributeErrors.ValueOrNumberRequired() : DomainResult.Success();

    private static DomainResult<short> CheckPosition(int position)
        => position is < 0 or > MaxPosition ? AttributeErrors.PositionInvalid() : (short)position;
}
