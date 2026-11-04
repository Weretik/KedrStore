using Content.Application.Features.Articles.Common;

namespace Content.Api.Articles;

internal static class ArticleResponseMapper
{
    public static string? CoverPath(long id, ArticleCoverView cover)
        => cover.LegacyVersion is { } v ? $"/api/content/articles/{id}/cover?v={v}" : null;

    public static ArticleSummaryResponse ToResponse(PublicArticleListItem item) => new(
        item.Id, item.Slug, item.Title, item.Excerpt, item.PublishedOn, CoverPath(item.Id, item.Cover), item.Cover.Url,
        item.UpdatedAt);

    public static ArticleResponse ToResponse(PublicArticleDetails item) => new(
        item.Id, item.Slug, item.Title, item.Excerpt, item.BodyHtml, item.PublishedOn,
        CoverPath(item.Id, item.Cover), item.Cover.Url, item.UpdatedAt);

    public static AdminArticleSummaryResponse ToResponse(AdminArticleListItem item) => new(
        item.Id, item.Slug, item.Title, item.PublishedOn, item.IsPublished, CoverPath(item.Id, item.Cover), item.Cover.Url,
        item.UpdatedAt, item.UpdatedBy);

    public static AdminArticleResponse ToResponse(AdminArticleDetails item) => new(
        item.Id, item.Slug, item.PublishedOn, item.IsPublished, CoverPath(item.Id, item.Cover), item.Cover.Url,
        item.Cover.MediaFileId, item.UpdatedAt, item.UpdatedBy,
        item.Texts.Select(text => new ArticleTextDto(text.Language, text.Title, text.Excerpt, text.BodyHtml)).ToList());

    public static ArticleCoverUploadResponse ToResponse(UploadedArticleCover cover) => new(cover.MediaFileId, cover.Url);

    public static ArticleInput ToInput(SaveArticleRequest request) => new(
        request.Slug, request.PublishedOn, request.IsPublished,
        (request.Texts ?? []).Select(text => new ArticleTextInput(text.Language, text.Title, text.Excerpt, text.BodyHtml)).ToList(),
        request.CoverMediaFileId, request.RemoveCover);
}
