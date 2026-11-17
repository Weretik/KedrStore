namespace Content.Domain.Requisites;

/// <summary>Database identity of a change of the company requisites.</summary>
public readonly record struct CompanyRequisitesChangeId : IComparable<CompanyRequisitesChangeId>
{
    public long Value { get; }

    private CompanyRequisitesChangeId(long value) => Value = value;

    public static CompanyRequisitesChangeId Create(long value) => value > 0
        ? new CompanyRequisitesChangeId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static CompanyRequisitesChangeId FromStorage(long value) => new(value);

    public int CompareTo(CompanyRequisitesChangeId other) => Value.CompareTo(other.Value);
}
