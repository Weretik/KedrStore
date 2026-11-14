using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

/// <summary>Published articles of one language, newest first.</summary>
public sealed record GetPublicArticlesQuery(string Language) : IQuery<Result<IReadOnlyList<PublicArticleListItem>>>;
