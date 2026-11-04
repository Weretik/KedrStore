namespace Content.Domain.Articles;

public static class ArticleErrors
{
    public static DomainError SlugInvalid(string? slug) =>
        new("Content.Article.Slug.Invalid",
            $"Slug must use a-z, 0-9 and single hyphens, 1-{Article.SlugMaxLength} characters. Actual: {slug}");

    public static DomainError LanguageMissing(string language) =>
        new("Content.Article.Translation.Missing", $"The '{language}' text of the article is required");

    public static DomainError LanguageInvalid(string? language) =>
        new("Content.Article.Translation.Language", $"Article language must be 'uk' or 'ru'. Actual: {language}");

    public static DomainError Required(string field) =>
        new($"Content.Article.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Article.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");
}
