using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetList;

/// <summary>Published articles of one language, newest first.</summary>
public sealed record GetPublicArticlesQuery(string Language) : IQuery<Result<IReadOnlyList<PublicArticleListItem>>>;
