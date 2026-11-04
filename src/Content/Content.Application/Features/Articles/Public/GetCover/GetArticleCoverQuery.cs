using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetCover;

/// <summary>Cover bytes of an article; the admin preview uses it too, so drafts are included.</summary>
public sealed record GetArticleCoverQuery(long ArticleId) : IQuery<Result<ArticleCoverFile>>;
