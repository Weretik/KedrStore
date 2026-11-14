using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Domain.Articles;

public static class ArticleErrors
{
    public static DomainError SlugInvalid() =>
        new("Content.Article.Slug.Invalid",
            $"Лише a-z, 0-9 та одинарні дефіси, до {Article.SlugMaxLength} символів.", Field: "Slug");

    public static DomainError LanguageMissing(string language) =>
        new("Content.Article.Translation.Missing",
            language == "ru" ? "Додайте текст російською." : "Додайте текст українською.", Field: "Texts");

    public static DomainError LanguageInvalid() =>
        new("Content.Article.Translation.Language", "Мова статті: лише українська або російська.", Field: "Texts");

    public static DomainError CoverInvalid() =>
        new("Content.Article.Cover.Invalid", "Оберіть зображення з медіатеки.", Field: "CoverMediaFileId");

    public static DomainError Required(string field) =>
        new($"Content.Article.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Article.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);
}
