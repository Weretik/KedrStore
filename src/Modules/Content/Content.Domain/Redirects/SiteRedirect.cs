using Content.Domain.Editing;

namespace Content.Domain.Redirects;

/// <summary>
/// A permanent (301) redirect of the public site from an old address to a new one, which an editor keeps in the
/// admin «Переадресації». The site answers every request to <see cref="FromPath"/> with a 301 to <see cref="ToUrl"/>.
/// </summary>
public sealed class SiteRedirect : IAggregateRoot
{
    /// <summary>The site loads every redirect at once, so their number stays small.</summary>
    public const int MaxCount = 2000;

    public SiteRedirectId Id { get; private set; }
    public string FromPath { get; private set; } = null!;
    public string ToUrl { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; } = null!;
    /// <summary>Row version (PostgreSQL <c>xmin</c>): a save over newer data answers 409.</summary>
    public uint Version { get; private set; }

    private SiteRedirect() { }

    public static DomainResult<SiteRedirect> Create(string? from, string? to, DateTimeOffset now, string? editor)
    {
        var redirect = new SiteRedirect { CreatedAt = now };
        var changed = redirect.Change(from, to, now, editor);
        return changed.IsSuccess ? redirect : changed.Error!;
    }

    public DomainResult Change(string? from, string? to, DateTimeOffset now, string? editor)
    {
        var fromPath = SiteRedirectPath.NormalizeFrom(from);
        var toUrl = SiteRedirectPath.NormalizeTo(to);
        var valid = DomainResult.Combine(fromPath, toUrl);
        if (valid.IsFailure)
            return valid;
        if (SiteRedirectPath.KeyOfTarget(toUrl.Value) == fromPath.Value)
            return SiteRedirectErrors.SameAddress();

        FromPath = fromPath.Value;
        ToUrl = toUrl.Value;
        UpdatedAt = now;
        UpdatedBy = EditorName.Normalize(editor);
        return DomainResult.Success();
    }
}
