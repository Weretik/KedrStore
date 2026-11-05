namespace Content.Domain.Seo;

public static class PageSeoErrors
{
    public static DomainError UnknownPage(string page) =>
        new("Content.PageSeo.Page.Unknown", $"Unknown site page: {page}");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.PageSeo.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError UnknownLanguage(string language) =>
        new("Content.PageSeo.Language.Unknown", $"Unknown language: {language}");
}
