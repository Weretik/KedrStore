namespace Content.Application.Features.Articles.Common;

internal static class ArticleMapper
{
    public static AdminArticleDetails ToAdmin(Article article, ArticleCoverView cover) => new(
        article.Id.Value, article.Slug, article.PublishedOn, article.IsPublished, cover, article.UpdatedAt,
        article.UpdatedBy,
        article.Translations.Select(t => new ArticleTextInput(t.Language, t.Title, t.Excerpt, t.BodyHtml)).ToList());
}
