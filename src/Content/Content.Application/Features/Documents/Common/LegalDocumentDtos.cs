namespace Content.Application.Features.Documents.Common;

public sealed record LegalDocumentTextData(string Language, string Title, string BodyHtml);

/// <summary>The published version of a document in one language, as the site shows it.</summary>
public sealed record PublishedLegalDocument(LegalDocumentKind Kind, int Number, DateOnly EffectiveFrom,
    string Title, string BodyHtml, DateTimeOffset PublishedAt);

/// <summary>Short facts of one version for lists.</summary>
public sealed record LegalDocumentVersionSummary(long Id, LegalDocumentStatus Status, int? Number,
    DateOnly? EffectiveFrom, DateTimeOffset UpdatedAt, string UpdatedBy, DateTimeOffset? PublishedAt,
    string? PublishedBy);

/// <summary>One document kind in the admin list: what is on the site now and whether a draft is waiting.</summary>
public sealed record LegalDocumentOverview(LegalDocumentKind Kind, LegalDocumentVersionSummary? Published,
    LegalDocumentVersionSummary? Draft);

public sealed record LegalDocumentVersionDetails(long Id, LegalDocumentKind Kind, LegalDocumentStatus Status,
    int? Number, DateOnly? EffectiveFrom, DateTimeOffset UpdatedAt, string UpdatedBy, DateTimeOffset? PublishedAt,
    string? PublishedBy, IReadOnlyList<LegalDocumentTextData> Texts);

internal static class LegalDocumentMapper
{
    public static LegalDocumentVersionSummary ToSummary(LegalDocumentVersion version) => new(version.Id,
        version.Status, version.Number, version.EffectiveFrom, version.UpdatedAt, version.UpdatedBy,
        version.PublishedAt, version.PublishedBy);

    public static LegalDocumentVersionDetails ToDetails(LegalDocumentVersion version) => new(version.Id,
        version.Kind, version.Status, version.Number, version.EffectiveFrom, version.UpdatedAt, version.UpdatedBy,
        version.PublishedAt, version.PublishedBy,
        version.Texts.Select(text => new LegalDocumentTextData(text.Language, text.Title, text.BodyHtml)).ToList());
}
