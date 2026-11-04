using Content.Application.Contracts.Html;

namespace Content.Application.Features.Documents.Common;

internal static class LegalDocumentEditor
{
    public static IReadOnlyCollection<LegalDocumentTextInput> Sanitized(IEnumerable<LegalDocumentTextData> texts,
        IArticleHtmlSanitizer sanitizer) => texts
        .Select(text => new LegalDocumentTextInput(text.Language, text.Title, sanitizer.Sanitize(text.BodyHtml)))
        .ToList();
}
