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

    internal static LegalDocumentText Create(LegalDocumentTextInput input)
    {
        var language = input.Language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!LegalDocumentVersion.Languages.Contains(language))
            throw new DomainException(LegalDocumentErrors.LanguageInvalid(input.Language));

        return new LegalDocumentText
        {
            Language = language,
            Title = Required(input.Title, TitleMaxLength, nameof(Title)),
            BodyHtml = Required(input.BodyHtml, BodyHtmlMaxLength, nameof(BodyHtml)),
        };
    }

    private static string Required(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(LegalDocumentErrors.Required(field));

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(LegalDocumentErrors.TooLong(field, maxLength, trimmed.Length));

        return trimmed;
    }
}

/// <summary>Input for one language; <see cref="BodyHtml"/> must already be sanitized.</summary>
public sealed record LegalDocumentTextInput(string Language, string Title, string BodyHtml);
