namespace Content.Domain.Documents;

/// <summary>
/// One edition of a legal document in Ukrainian and Russian. A draft is edited freely; publishing gives it the
/// next number and the «Редакція від» date and makes it read-only; the version it replaces is archived.
/// </summary>
public sealed class LegalDocumentVersion : IAggregateRoot
{
    public const int UserNameMaxLength = 256;
    public static readonly IReadOnlyList<string> Languages = ["uk", "ru"];

    private readonly List<LegalDocumentText> _texts = [];

    public long Id { get; private set; }
    public LegalDocumentKind Kind { get; private set; }
    public LegalDocumentStatus Status { get; private set; }

    /// <summary>1, 2, 3… in publishing order; <c>null</c> while a draft.</summary>
    public int? Number { get; private set; }

    /// <summary>The «Редакція від» date shown on the site; <c>null</c> while a draft.</summary>
    public DateOnly? EffectiveFrom { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; } = null!;
    public DateTimeOffset? PublishedAt { get; private set; }
    public string? PublishedBy { get; private set; }
    public IReadOnlyList<LegalDocumentText> Texts => _texts;

    public bool IsDraft => Status == LegalDocumentStatus.Draft;

    private LegalDocumentVersion() { }

    /// <summary>«Редакція від» may not be earlier than the edition the site shows now.</summary>
    public static bool AllowsEffectiveFrom(DateOnly effectiveFrom, LegalDocumentVersion? current)
        => current?.EffectiveFrom is not { } currentDate || currentDate <= effectiveFrom;

    public static DomainResult<LegalDocumentVersion> CreateDraft(LegalDocumentKind kind,
        IReadOnlyCollection<LegalDocumentTextInput> texts, DateTimeOffset now, string userName)
    {
        var version = new LegalDocumentVersion { Kind = kind, Status = LegalDocumentStatus.Draft };
        var replaced = version.ReplaceTexts(texts, now, userName);
        return replaced.IsFailure ? replaced.Error! : version;
    }

    public DomainResult UpdateDraft(IReadOnlyCollection<LegalDocumentTextInput> texts, DateTimeOffset now,
        string userName)
        => IsDraft ? ReplaceTexts(texts, now, userName) : LegalDocumentErrors.NotDraft();

    /// <summary>Publishes this draft as <paramref name="number"/>; the caller archives the previous one.</summary>
    public DomainResult Publish(int number, DateOnly effectiveFrom, DateTimeOffset now, string userName)
    {
        // The number is the next one the repository counted, never user input.
        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);
        if (!IsDraft)
            return LegalDocumentErrors.NotDraft();

        Status = LegalDocumentStatus.Published;
        Number = number;
        EffectiveFrom = effectiveFrom;
        PublishedAt = now;
        PublishedBy = Trim(userName);
        return DomainResult.Success();
    }

    /// <summary>
    /// Publishes this draft as the next edition after <paramref name="lastNumber"/> and archives
    /// <paramref name="current"/>, the edition of the same kind the site shows now.
    /// </summary>
    public DomainResult PublishReplacing(LegalDocumentVersion? current, int lastNumber, DateOnly effectiveFrom,
        DateTimeOffset now, string userName)
    {
        if (current is not null && current.Kind != Kind)
            throw new ArgumentException("The current edition must be of the same document kind.", nameof(current));
        if (!AllowsEffectiveFrom(effectiveFrom, current))
            return LegalDocumentErrors.EffectiveFromTooEarly(current!.EffectiveFrom!.Value);
        if (current is not null && current.Status != LegalDocumentStatus.Published)
            return LegalDocumentErrors.NotPublished();

        var published = Publish(lastNumber + 1, effectiveFrom, now, userName);
        if (published.IsFailure)
            return published;
        return current?.Archive() ?? DomainResult.Success();
    }

    public DomainResult Archive()
    {
        if (Status != LegalDocumentStatus.Published)
            return LegalDocumentErrors.NotPublished();
        Status = LegalDocumentStatus.Archived;
        return DomainResult.Success();
    }

    public LegalDocumentText Text(string language) => _texts.Single(text => text.Language == language);

    private DomainResult ReplaceTexts(IReadOnlyCollection<LegalDocumentTextInput> texts, DateTimeOffset now,
        string userName)
    {
        ArgumentNullException.ThrowIfNull(texts);
        var created = new List<LegalDocumentText>();
        foreach (var input in texts)
        {
            var text = LegalDocumentText.Create(input);
            if (text.IsFailure)
                return text.Error!;
            created.Add(text.Value);
        }

        foreach (var language in Languages)
        {
            if (created.Count(text => text.Language == language) != 1)
                return LegalDocumentErrors.LanguageMissing(language);
        }

        _texts.Clear();
        _texts.AddRange(created.OrderBy(text => text.Language, StringComparer.Ordinal));
        UpdatedAt = now;
        UpdatedBy = Trim(userName);
        return DomainResult.Success();
    }

    private static string Trim(string userName)
    {
        var name = string.IsNullOrWhiteSpace(userName) ? "unknown" : userName.Trim();
        return name.Length <= UserNameMaxLength ? name : name[..UserNameMaxLength];
    }
}
