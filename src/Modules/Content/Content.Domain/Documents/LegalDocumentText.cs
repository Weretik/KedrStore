namespace Content.Domain.Documents;

/// <summary>Title and sanitized body of a version in one site language.</summary>
public sealed class LegalDocumentText
{
    public const int TitleMaxLength = 200;
    public const int BodyHtmlMaxLength = 100_000;

    public string Language { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string BodyHtml { get; private set; } = null!;

    private LegalDocumentText() { }

    internal static DomainResult<LegalDocumentText> Create(LegalDocumentTextInput input)
    {
        var language = input.Language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!LegalDocumentVersion.Languages.Contains(language))
            return LegalDocumentErrors.LanguageInvalid();

        var checks = new DomainChecks();
        var title = checks.Take(Required(input.Title, TitleMaxLength, nameof(Title)));
        var bodyHtml = checks.Take(Required(input.BodyHtml, BodyHtmlMaxLength, nameof(BodyHtml)));
        if (checks.Error is { } error)
            return error;

        return new LegalDocumentText { Language = language, Title = title, BodyHtml = bodyHtml };
    }

    private static DomainResult<string> Required(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return LegalDocumentErrors.Required(field);

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? LegalDocumentErrors.TooLong(field, maxLength, trimmed.Length) : trimmed;
    }
}

/// <summary>Input for one language; <see cref="BodyHtml"/> must already be sanitized.</summary>
public sealed record LegalDocumentTextInput(string Language, string Title, string BodyHtml);
