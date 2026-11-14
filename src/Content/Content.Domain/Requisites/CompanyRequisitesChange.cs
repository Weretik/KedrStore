namespace Content.Domain.Requisites;

/// <summary>
/// Log entry of one saved change of the requisites: who, when, and the old and new value of each changed field.
/// Entries are only added, never edited or deleted.
/// </summary>
public sealed class CompanyRequisitesChange : IAggregateRoot
{
    public const int UserNameMaxLength = 256;

    private readonly List<RequisitesFieldChange> _changes = [];

    public long Id { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string ChangedByUserId { get; private set; } = null!;

    /// <summary>Email or user name at the time of the change, kept even if the account is renamed.</summary>
    public string ChangedByName { get; private set; } = null!;
    public IReadOnlyList<RequisitesFieldChange> Changes => _changes;

    private CompanyRequisitesChange() { }

    public static CompanyRequisitesChange Record(IReadOnlyList<RequisitesFieldChange> changes,
        DateTimeOffset changedAt, string userId, string userName)
    {
        // The handler records only a save that changed something, so an empty list is a caller bug.
        if (changes.Count == 0)
            throw new ArgumentException("A change log entry needs at least one changed field.", nameof(changes));
        var entry = new CompanyRequisitesChange
        {
            ChangedAt = changedAt,
            ChangedByUserId = userId,
            ChangedByName = userName.Length > UserNameMaxLength ? userName[..UserNameMaxLength] : userName,
        };
        entry._changes.AddRange(changes);
        return entry;
    }
}
