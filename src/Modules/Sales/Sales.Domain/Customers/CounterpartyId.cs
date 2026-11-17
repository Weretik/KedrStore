namespace Sales.Domain.Customers;

/// <summary>Identity of a counterparty: its 1C code.</summary>
public readonly record struct CounterpartyId : IComparable<CounterpartyId>
{
    public string Value { get; }

    private CounterpartyId(string value) => Value = value;

    public static CounterpartyId Create(string value) => string.IsNullOrWhiteSpace(value)
        ? throw new ArgumentException("A counterparty id is required.", nameof(value))
        : new CounterpartyId(value.Trim());

    public static CounterpartyId FromStorage(string value) => new(value);

    public int CompareTo(CounterpartyId other) => string.CompareOrdinal(Value, other.Value);

    public override string ToString() => Value;
}
