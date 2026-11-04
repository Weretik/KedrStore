using Content.Application.Features.Documents.Common;
using Content.Domain.Documents;

namespace Content.Api.Documents;

/// <summary>
/// The published edition of a document in one language. <c>bodyHtml</c> is sanitized HTML (h2/h3, p, lists,
/// links) and may hold <c>{site}</c>, <c>{email}</c> and <c>{phone}</c>, which the site fills in.
/// </summary>
public sealed record PublishedLegalDocumentResponse(
    string Kind, int Number, DateOnly EffectiveFrom, string Title, string BodyHtml, DateTimeOffset PublishedAt);

/// <summary>Short facts of one version; <c>status</c> is <c>draft</c>, <c>published</c> or <c>archived</c>.</summary>
public sealed record LegalDocumentVersionSummaryResponse(
    long Id, string Status, int? Number, DateOnly? EffectiveFrom, DateTimeOffset UpdatedAt, string UpdatedBy,
    DateTimeOffset? PublishedAt, string? PublishedBy);

/// <summary>One document in the admin list: the edition on the site and the waiting draft, if any.</summary>
public sealed record LegalDocumentOverviewResponse(
    string Kind, LegalDocumentVersionSummaryResponse? Published, LegalDocumentVersionSummaryResponse? Draft);

public sealed record LegalDocumentTextResponse(string Language, string Title, string BodyHtml);

public sealed record LegalDocumentVersionResponse(
    long Id, string Kind, string Status, int? Number, DateOnly? EffectiveFrom, DateTimeOffset UpdatedAt,
    string UpdatedBy, DateTimeOffset? PublishedAt, string? PublishedBy, IReadOnlyList<LegalDocumentTextResponse> Texts);

/// <summary>Both languages at once; the HTML is sanitized on save.</summary>
public sealed record SaveLegalDocumentDraftRequest(IReadOnlyList<LegalDocumentTextResponse>? Texts);

/// <summary>«Редакція від»: the date the edition applies from.</summary>
public sealed record PublishLegalDocumentRequest(DateOnly EffectiveFrom);

internal static class LegalDocumentResponseMapper
{
    public static PublishedLegalDocumentResponse ToResponse(PublishedLegalDocument document) => new(
        LegalDocumentKinds.ToSlug(document.Kind), document.Number, document.EffectiveFrom, document.Title,
        document.BodyHtml, document.PublishedAt);

    public static LegalDocumentOverviewResponse ToResponse(LegalDocumentOverview overview) => new(
        LegalDocumentKinds.ToSlug(overview.Kind),
        overview.Published is null ? null : ToResponse(overview.Published),
        overview.Draft is null ? null : ToResponse(overview.Draft));

    public static LegalDocumentVersionSummaryResponse ToResponse(LegalDocumentVersionSummary version) => new(
        version.Id, Status(version.Status), version.Number, version.EffectiveFrom, version.UpdatedAt,
        version.UpdatedBy, version.PublishedAt, version.PublishedBy);

    public static LegalDocumentVersionResponse ToResponse(LegalDocumentVersionDetails version) => new(
        version.Id, LegalDocumentKinds.ToSlug(version.Kind), Status(version.Status), version.Number,
        version.EffectiveFrom, version.UpdatedAt, version.UpdatedBy, version.PublishedAt, version.PublishedBy,
        version.Texts.Select(text => new LegalDocumentTextResponse(text.Language, text.Title, text.BodyHtml)).ToList());

    public static IReadOnlyList<LegalDocumentTextData>? ToInput(SaveLegalDocumentDraftRequest? request) =>
        request?.Texts?.Select(text => new LegalDocumentTextData(text.Language, text.Title, text.BodyHtml)).ToList();

    private static string Status(LegalDocumentStatus status) => status.ToString().ToLowerInvariant();
}
