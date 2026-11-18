namespace Content.Domain.Contacts;

/// <summary>Database identity of the company contacts row.</summary>
public readonly record struct CompanyContactsId : IComparable<CompanyContactsId>
{
    public int Value { get; }

    private CompanyContactsId(int value) => Value = value;

    public static CompanyContactsId Create(int value) => value > 0
        ? new CompanyContactsId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static CompanyContactsId FromStorage(int value) => new(value);

    public int CompareTo(CompanyContactsId other) => Value.CompareTo(other.Value);
}
