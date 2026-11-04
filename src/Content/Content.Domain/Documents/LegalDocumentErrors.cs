namespace Content.Domain.Documents;

public static class LegalDocumentErrors
{
    public static DomainError LanguageMissing(string language) =>
        new("Content.Document.Text.Missing", $"The '{language}' text of the document is required");

    public static DomainError LanguageInvalid(string? language) =>
        new("Content.Document.Text.Language", $"Document language must be 'uk' or 'ru'. Actual: {language}");

    public static DomainError Required(string field) =>
        new($"Content.Document.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Document.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError NotDraft(long id) =>
        new("Content.Document.NotDraft", $"Version {id} is already published; only a draft can be changed");

    public static DomainError NotPublished(long id) =>
        new("Content.Document.NotPublished", $"Version {id} is not the published one");

    public static DomainError NumberInvalid(int number) =>
        new("Content.Document.Number.Invalid", $"A version number must be positive. Actual: {number}");
}
