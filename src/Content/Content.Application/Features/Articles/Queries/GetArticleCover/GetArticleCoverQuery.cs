using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetArticleCover;

/// <summary>Cover bytes of an article; the admin preview uses it too, so drafts are included.</summary>
public sealed record GetArticleCoverQuery(long ArticleId) : IQuery<Result<ArticleCoverFile>>;
