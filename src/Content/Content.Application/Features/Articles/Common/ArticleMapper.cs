namespace Content.Application.Features.Articles.Common;

internal static class ArticleMapper
{
    public static AdminArticleDetails ToAdmin(Article article) => new(
        article.Id.Value, article.Slug, article.PublishedOn, article.IsPublished, article.CoverVersion, article.UpdatedAt,
        article.Translations.Select(t => new ArticleTextInput(t.Language, t.Title, t.Excerpt, t.BodyHtml)).ToList());
}
