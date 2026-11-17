namespace Content.Domain.Documents;

/// <summary>Database identity of a legal document version.</summary>
public readonly record struct LegalDocumentVersionId : IComparable<LegalDocumentVersionId>
{
    public long Value { get; }

    private LegalDocumentVersionId(long value) => Value = value;

    public static LegalDocumentVersionId Create(long value) => value > 0
        ? new LegalDocumentVersionId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static LegalDocumentVersionId FromStorage(long value) => new(value);

    public int CompareTo(LegalDocumentVersionId other) => Value.CompareTo(other.Value);
}
