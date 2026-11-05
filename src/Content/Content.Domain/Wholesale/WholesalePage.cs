using Content.Domain.Editing;

namespace Content.Domain.Wholesale;

/// <summary>
/// The editable copy of the site page «Співпраця» (wholesale offer) in both languages. There is exactly one row
/// (<see cref="SingletonId"/>), created by a migration with the texts the site kept in code, and only updated.
/// Buttons, links, and contact values are not here: they stay in the site or come from the company contacts.
/// </summary>
public sealed class WholesalePage : IAggregateRoot
{
    public const int SingletonId = 1;
    public static readonly IReadOnlyList<string> Languages = ["uk", "ru"];

    public int Id { get; private set; }
    public WholesalePageText Uk { get; private set; } = null!;
    public WholesalePageText Ru { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Who saved the last change; empty for the row created by the migration.</summary>
    public string? UpdatedBy { get; private set; }

    private WholesalePage() { }

    public static WholesalePage Create(WholesalePageText uk, WholesalePageText ru, DateTimeOffset now)
    {
        var page = new WholesalePage { Id = SingletonId };
        page.Apply(uk, ru, now);
        return page;
    }

    public WholesalePageText Text(string language) => language switch
    {
        "uk" => Uk,
        "ru" => Ru,
        _ => throw new DomainException(WholesaleErrors.UnknownLanguage(language)),
    };

    public void Update(WholesalePageText uk, WholesalePageText ru, DateTimeOffset now, string editor)
    {
        Apply(uk, ru, now);
        UpdatedBy = EditorName.Normalize(editor);
    }

    private void Apply(WholesalePageText uk, WholesalePageText ru, DateTimeOffset now)
    {
        Uk = uk ?? throw new ArgumentNullException(nameof(uk));
        Ru = ru ?? throw new ArgumentNullException(nameof(ru));
        UpdatedAt = now;
    }
}
