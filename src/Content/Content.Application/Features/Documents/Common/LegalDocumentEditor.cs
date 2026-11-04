using BuildingBlocks.Application.Contracts;
using Content.Application.Contracts.Html;

namespace Content.Application.Features.Documents.Common;

internal static class LegalDocumentEditor
{
    /// <summary>The editor's email, else user name or id, as stored with a version.</summary>
    public static string Name(ICurrentUserService user) =>
        new[] { user.Email, user.UserName, user.UserId }.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
        ?? "unknown";

    public static IReadOnlyCollection<LegalDocumentTextInput> Sanitized(IEnumerable<LegalDocumentTextData> texts,
        IArticleHtmlSanitizer sanitizer) => texts
        .Select(text => new LegalDocumentTextInput(text.Language, text.Title, sanitizer.Sanitize(text.BodyHtml)))
        .ToList();
}
