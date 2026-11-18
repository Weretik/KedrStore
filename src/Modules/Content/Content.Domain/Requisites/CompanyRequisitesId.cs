namespace Content.Domain.Requisites;

/// <summary>Database identity of the company requisites row.</summary>
public readonly record struct CompanyRequisitesId : IComparable<CompanyRequisitesId>
{
    public int Value { get; }

    private CompanyRequisitesId(int value) => Value = value;

    public static CompanyRequisitesId Create(int value) => value > 0
        ? new CompanyRequisitesId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static CompanyRequisitesId FromStorage(int value) => new(value);

    public int CompareTo(CompanyRequisitesId other) => Value.CompareTo(other.Value);
}
