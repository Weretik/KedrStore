namespace BuildingBlocks.Domain.Identifiers;

/// <summary>Catalogue price type (its 1C code); Sales customers link to their price types by this ID.</summary>
public readonly record struct PriceTypeId : IComparable<PriceTypeId>, IComparable
{
    public const int MaxValue = 1_000_000;

    public int Value { get; }

    private PriceTypeId(int value) => Value = value;

    /// <summary>A 1C code: greater than zero and at most <see cref="MaxValue"/>.</summary>
    public static PriceTypeId Create(int value) => value is > 0 and <= MaxValue
        ? new PriceTypeId(value)
        : throw new ArgumentOutOfRangeException(nameof(value), value, "The code must be between 1 and " + MaxValue + ".");

    public static PriceTypeId FromStorage(int value) => new(value);

    public int CompareTo(PriceTypeId other) => Value.CompareTo(other.Value);

    int IComparable.CompareTo(object? obj) => obj is PriceTypeId other ? CompareTo(other)
        : obj is null ? 1 : throw new ArgumentException("Expected " + nameof(PriceTypeId) + ".", nameof(obj));

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
